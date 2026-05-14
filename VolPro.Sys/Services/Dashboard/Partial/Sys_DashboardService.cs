/*
 *所有关于Sys_Dashboard类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*Sys_DashboardService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;
using System.Linq;
using VolPro.Core.Utilities;
using System.Linq.Expressions;
using VolPro.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using VolPro.Sys.IRepositories;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using VolPro.Core.DBManager;
using System.Data;
using VolPro.Core.Services;
using VolPro.Core.ManageUser;
using VolPro.Core.Configuration;
using VolPro.Core.DbSqlSugar;
using SqlSugar;
using VolPro.Core.Dashboard;

namespace VolPro.Sys.Services
{
    public partial class Sys_DashboardService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISys_DashboardRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public Sys_DashboardService(
            ISys_DashboardRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }
        public override PageGridData<Sys_Dashboard> GetPageData(PageDataOptions options)
        {
            QueryRelativeExpression = (ISugarQueryable<Sys_Dashboard> query) =>
            {
                if (!UserContext.Current.IsSuperAdmin && AppSetting.UseDynamicShareDB)
                {
                    query = query.Where(x => x.DbServiceId == UserContext.CurrentServiceId);
                }
                return query;
            };
            return base.GetPageData(options);
        }
        WebResponseContent webResponse = new WebResponseContent();
        public override WebResponseContent Add(SaveModel saveDataModel)
        {
            AddOnExecuting = (Sys_Dashboard dashboard, object list) =>
            {
                dashboard.DashboardId = Guid.NewGuid();
                dashboard.DashboardCode = dashboard.DashboardId.ToString();
                dashboard.DbServiceId = UserContext.CurrentServiceId;
                dashboard.TenancyId = UserContext.CurrentServiceId.ToString();
                return webResponse.OK();
            };
            var result = base.Add(saveDataModel);
            if (result.Status)
            {
                RemoveCache();
            }
            return result;
        }

        public override WebResponseContent Update(SaveModel saveModel)
        {
            var result = base.Update(saveModel);
            if (result.Status)
            {
                RemoveCache();
            }
            return result;
        }

        public override WebResponseContent Del(object[] keys, bool delList = true)
        {
            RemoveCache();
            return base.Del(keys, delList);
        }

        /// <summary>
        /// 编译、预览、查看获取全部配置
        /// </summary>
        /// <param name="id"></param>
        /// <param name="view"></param>
        /// <returns></returns>

        public async Task<object> GetLayoutData(Guid id, bool view)
        {
            var data = await repository.FindAsIQueryable(x => x.DashboardId == id).FirstOrDefaultAsync();
            return data;
        }
        /// <summary>
        ///  获取每项sql数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public async Task<object> GetItemData(Guid id, string itemId, DateTime? date1, DateTime? date2, string filterType)
        {
            var info = GetAllCacheData().Where(x => x.DashboardId == id).FirstOrDefault();
            if (info == null)
            {
                return null;
            }
            var item = info.DashboardItems.Where(x => x.ItemId == itemId).FirstOrDefault();
            if (item == null)
            {
                return null;
            }
            return await ExecSql(item.Sql, item.DbService, item.IsProc, date1, date2, filterType);
        }

        private async Task<object> ExecSql(string sql, string dbService, bool isProc, DateTime? date1, DateTime? date2, string filterType)
        {
            if (date1 == null && date2 == null && !string.IsNullOrEmpty(filterType))
            {
                switch (filterType)
                {
                    case "今天":
                        date1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                        date2 = DateTime.Now;
                        break;
                    case "近7天":
                        date1 = DateTime.Today.AddDays(-6);
                        date2 = DateTime.Now;
                        break;
                    case "本月":
                        date1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        date2 = DateTime.Now.AddDays(1); ;
                        break;
                    case "近1月":
                        date1 = DateTime.Today.AddMonths(-1).AddDays(1);
                        date2 = DateTime.Now.AddMonths(1);
                        break;
                    case "近半年":
                        date1 = DateTime.Today.AddMonths(-6).AddMonths(1);
                        date2 = DateTime.Now.AddMonths(1);
                        break;
                    case "近一年":
                        date1 = DateTime.Today.AddYears(-1).AddMonths(1);
                        date2 = DateTime.Now.AddMonths(1);
                        break;
                    default:
                        break;
                }
            }
            try
            {
                sql = FilterSql(sql);
                if (date1 == null)
                {
                    date1 = DateTime.Now.AddYears(-20);
                }
                if (date2 == null)
                {
                    date2 = DateTime.Now.AddDays(2);
                }
                var ado = DbManger.GetConnection(dbService).Ado;
                if (isProc)
                {
                    ado = ado.UseStoredProcedure();
                }
                var parameters = new List<SugarParameter>(){
                  new SugarParameter("@date1",date1),
                  new SugarParameter("@date2",date2)
                };
                IDashboardFilterMetaData filter = new DashboardFilter();
                var res = filter.OnActionExecuting(sql, parameters, dbService, isProc, date1, date2, filterType);
                var result = await ado.SqlQueryAsync<object>(res.sql, res.parameters);
                return result;
            }
            catch (Exception ex)
            {
                string message = $"sql执行{(isProc ? "存储过程" : "")}异常,：{sql},{ex.Message + ex.InnerException + ex.StackTrace}";
                Logger.Error(message);
                throw new Exception(message);
            }
        }


        /// <summary>
        /// 编译执行sql
        /// </summary>
        /// <param name="id"></param>
        /// <param name="view"></param>
        /// <returns></returns>

        public async Task<object> ExecSql(Dictionary<string, string> dic)
        {
            return await ExecSql(dic["sql"],
                GetValue(dic, "db"),
                GetValue(dic, "isProc") == "1",
                GetValue(dic, "date1").GetDateTime(),
                GetValue(dic, "date2").GetDateTime(),
             GetValue(dic, "filterType"));
        }
        private string[] illegalKeywords = { " delete ", " drop ", " truncate ", " update ", " insert ", " alter ", " create ", " grant ", " revoke ", " exec ", " execute ", " shutdown " };

        private string FilterSql(string sql)
        {
            foreach (string keyword in illegalKeywords)
            {
                sql = sql.Replace(keyword, "", StringComparison.OrdinalIgnoreCase);
            }
            return sql;
        }


        /// <summary>
        /// 当前服务器的菜单版本
        /// </summary>
        private static string _DashboardVersionn = "";

        private const string _DashboardCacheKey = "DashboardKey";

        private static List<DashboardInfo> sysDashboards = new List<DashboardInfo>();
        private static object _DashboardObj = new object();


        private string GetValue(Dictionary<string, string> dic, string key)
        {
            if (!dic.TryGetValue(key, out string vlaue))
            {
                return null;
            }
            return vlaue?.ToString();
        }

        private string GetObjectValue(Dictionary<string, object> dic, string key)
        {
            if (!dic.TryGetValue(key, out object vlaue))
            {
                return null;
            }
            return vlaue?.ToString();
        }

        private void RemoveCache()
        {
            CacheContext.Remove(_DashboardCacheKey);
        }
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>

        private List<DashboardInfo> GetAllCacheData()
        {
            string _cacheVersion = CacheContext.Get(_DashboardCacheKey);
            if (_DashboardVersionn != "" && _DashboardVersionn == _cacheVersion)
            {
                return sysDashboards ?? new List<DashboardInfo>();
            }
            lock (_DashboardObj)
            {
                if (_DashboardVersionn != "" && sysDashboards != null && _DashboardVersionn == _cacheVersion) return sysDashboards;
                //2020.12.27增加菜单界面上不显示，但可以分配权限
                sysDashboards = repository.FindAsIQueryable(x => true).ToList().Serialize().DeserializeObject<List<DashboardInfo>>();
                foreach (var item in sysDashboards)
                {
                    if (string.IsNullOrEmpty(item.Options))
                    {
                        item.DashboardItems = new List<DashboardItem>();
                    }
                    else
                    {
                        item.DashboardItems = item.Options.DeserializeObject<List<Dictionary<string, object>>>()
                           .Select(dic => new DashboardItem()
                           {
                               ItemId = GetObjectValue(dic, "i"),
                               Sql = GetObjectValue(dic, "sql"),
                               DbService = GetObjectValue(dic, "db"),
                               IsProc = GetObjectValue(dic, "isProc") == "1"
                           }).ToList();
                    }
                }

                string cacheVersion = CacheContext.Get(_DashboardCacheKey);
                if (string.IsNullOrEmpty(cacheVersion))
                {
                    cacheVersion = DateTime.Now.ToString("yyyyMMddHHMMssfff");
                    CacheContext.Add(_DashboardCacheKey, cacheVersion);
                }
                else
                {
                    _DashboardVersionn = cacheVersion;
                }
            }
            return sysDashboards;
        }
    }

    public class DashboardInfo : Sys_Dashboard
    {

        public List<DashboardItem> DashboardItems;

    }

    public class DashboardItem
    {
        public string ItemId { get; set; }
        public string Sql { get; set; }
        public string DbService { get; set; }

        public bool IsProc { get; set; }
    }
}
