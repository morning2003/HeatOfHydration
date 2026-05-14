/*
 *所有关于V_Hoh_Project_Data类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*V_Hoh_Project_DataService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using Hncdi.HeatOfHydration.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using VolPro.Core.Extensions;
using VolPro.Core.Utilities;
using VolPro.Entity.DomainModels;

namespace Hncdi.HeatOfHydration.Services
{
    public partial class V_Hoh_Project_DataService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IV_Hoh_Project_DataRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public V_Hoh_Project_DataService(
            IV_Hoh_Project_DataRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }
        public override WebResponseContent Import(List<IFormFile> files)
        {
            ImportOnExecuting = (List<V_Hoh_Project_Data> list) =>
            {
                var webResponse = new WebResponseContent();
                try
                {
                    if (list == null || list.Count == 0)
                    {
                        webResponse.Code = "-1";
                        webResponse.Message = "导入数据为空";
                        return webResponse;
                    }

                    var db = repository.SqlSugarClient;

                    // 1. 获取匹配的 Point 字典（不变）
                    var queryKeys = list.Select(x => new { x.HohProject_id, x.PointName }).ToList();
                    var pointList = db.Queryable<Hoh_Project_Point>()
                     .Where(x => queryKeys.Any(k =>
                         k.HohProject_id == x.HohProject_id
                         && k.PointName == x.PointName
                     ))
                     .Select(x => new { x.HohProject_id, x.PointName, x.PointId })
                     .ToList();

                    var pointDic = pointList.ToDictionary(x => $"{x.HohProject_id}_{x.PointName}", x => x.PointId);

                    // 2. 过滤出能匹配到 PointId 的数据
                    var validData = list
                        .Where(item => pointDic.ContainsKey($"{item.HohProject_id}_{item.PointName}"))
                        .Select(item => new
                        {
                            HohProject_id = item.HohProject_id,
                            PointId = pointDic[$"{item.HohProject_id}_{item.PointName}"],
                            TimeVal = item.TimeVal,
                            PointVal = item.PointVal
                        }).OrderBy(x => x.TimeVal)
                        .ToList();

                    if (!validData.Any())
                    {
                        webResponse.Code = "-1";
                        webResponse.Message = "未匹配到有效点位信息，导入失败";
                        return webResponse;
                    }

                    // 3. 批量查询【已存在的数据】（联合唯一键：项目ID + 点位ID + 时间）
                    var existData = db.Queryable<Hoh_Project_SubData>()
                        .Where(x => validData.Any(v =>
                            v.HohProject_id == x.HohProject_id &&
                            v.PointId == x.PointId &&
                            v.TimeVal == x.TimeVal))
                        .Select(x => new { x.HohProject_id, x.PointId, x.TimeVal })
                        .ToList();

                    // 转成哈希集合，快速判断是否存在
                    var existSet = new HashSet<string>(
                        existData.Select(x => $"{x.HohProject_id}_{x.PointId}_{x.TimeVal}")
                    );

                    // 4. 拆分：需要新增 / 需要更新
                    var insertList = new List<Hoh_Project_SubData>();
                    var updateList = new List<Hoh_Project_SubData>();

                    foreach (var item in validData)
                    {
                        var key = $"{item.HohProject_id}_{item.PointId}_{item.TimeVal}";
                        var model = new Hoh_Project_SubData
                        {
                            HohProject_id = item.HohProject_id,
                            PointId = item.PointId,
                            TimeVal = item.TimeVal,
                            PointVal = item.PointVal,
                            CreateDate=DateTime.Now,
                        };

                        if (existSet.Contains(key))
                        {
                            updateList.Add(model); // 已存在 → 更新
                        }
                        else
                        {
                            insertList.Add(model); // 不存在 → 新增
                        }
                    }

                    // 5. 批量执行
                    int insertCount = 0;
                    int updateCount = 0;

                    // 新增
                    if (insertList.Any())
                    {
                        insertCount = db.Insertable(insertList).ExecuteCommand();
                    }

                    // 更新（按 项目+点位+时间 作为条件，只更新 PointVal）
                    if (updateList.Any())
                    {
                        updateCount = db.Updateable(updateList)
                            .WhereColumns(x => new { x.HohProject_id, x.PointId, x.TimeVal }) // 匹配条件
                            .UpdateColumns(x => x.PointVal) // 只更新值
                            .ExecuteCommand();
                    }

                    // 6. 返回结果
                    webResponse.Code = "-1";
                    webResponse.Message = $"导入完成：共{list.Count}条，新增{insertCount}条，更新{updateCount}条";
                    return webResponse.OK(webResponse.Message);
                }
                catch (Exception ex)
                {
                    webResponse.Code = "-1";
                    webResponse.Message = $"导入失败：{ex.Message}";
                    return webResponse;
                }
            };
            return base.Import(files);
        }


        public override WebResponseContent Del(object[] keys, bool delList = true)
        {
            WebResponseContent webResponse = new WebResponseContent();
            var ids = keys.Select(s => s.ToString().GetLong()).ToList();
            repository.DbContext.Deleteable<Hoh_Project_SubData>().Where(x => ids.Contains(x.Monitor_id)).ExecuteCommand();

            return webResponse.OK("删除成功");
        }
        }
}
