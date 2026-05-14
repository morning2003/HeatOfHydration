using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VolPro.Core.CacheManager;
using VolPro.Core.Configuration;
using VolPro.Core.DBManager;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.Extensions;
using VolPro.Core.Filters;
using VolPro.Core.ManageUser;
using VolPro.Core.Utilities;
using VolPro.Entity.DomainModels;

namespace VolPro.WebApi.Controllers
{

    [JWTAuthorize, ApiController]
    [Route("api/db")]
    public class DbManagerController : Controller
    {
        [Route("exectue"), HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiActionPermission(ActionRolePermission.SuperAdmin)]
        public ActionResult Exectue([FromBody] TextInfo info)
        {
            if (!AppSetting.UseDynamicShareDB)
            {
                return Content($"只有动态分库才能执行脚本");
            }
            List<Task> tasks = new List<Task>();
            ConcurrentBag<string> result = new ConcurrentBag<string>();

            var list = DbCache.GetList().Select(s => new
            {
                DbServiceId = s.DbServiceId.ToString(),
                s.DatabaseName,
                DbServiceName = s.DbServiceName,
                ConnectionString = DbCache.GetConnectionString(s)
            }).ToList();

            if (list.Count == 0)
            {
                return Content($"没有配置数据库");
            }
            var db = DbCache.GetList().First().Serialize().DeserializeObject<Sys_DbService>();

            db.DatabaseName = "DB_Empty";
            //如果数据库都不在同一台服务器上，每个服务器都要有一个DB_Empty数据库,这里也应该也要根据ip去重循环，待完

            string emptyDb = "EmptyDbContext";

            if (list.Count > 0)
            {
                list.Add(new
                {
                    list[0].DbServiceId,
                    db.DatabaseName,
                    DbServiceName = "空数据库",
                    ConnectionString = DbCache.GetConnectionString(db, db.DatabaseName)
                });
            }
            int errorCount = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                string text = $"{(DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss"))},实例：{item.DbServiceName},数据库:{item.DatabaseName},";
                try
                {
                    SqlSugar.IAdo ado = DbManger.GetServiceDb((Guid)item.DbServiceId.GetGuid()).Ado;
                    ado.CommandTimeOut = 60 * 20;
                    ado.ExecuteCommand(info.Text, new { });
                    text = text + "执行成功";
                }
                catch (Exception ex)
                {
                    errorCount++;
                    text = text + "执行失败," + ex.Message + ex.StackTrace;
                }
                result.Add(text);
            }
            result.Add($" --------{(DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss"))}({UserContext.Current.UserTrueName})------------ ");
            result.Add($"执行sql({UserContext.Current.UserTrueName})：{info.Text}");
            result.Add("==========================================");
            result.Add("\r\n");
            FileHelper.WriteFile("DbLogger//DbExecute".MapPath(), DateTime.Now.ToString("yyyy-MM-dd") + ".txt", string.Join("\r\n", result), true);

            return Content($"执行成功:{list.Count - errorCount}个,失败：{errorCount}个");
        }
    }

    public class TextInfo
    {
        public string Text { get; set; }

    }
}