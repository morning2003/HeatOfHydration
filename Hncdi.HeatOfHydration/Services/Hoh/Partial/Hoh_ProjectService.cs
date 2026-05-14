/*
 *所有关于Hoh_Project类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*Hoh_ProjectService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using CDynamic.Command.Defaults;
using Hncdi.HeatOfHydration.IRepositories;
using Hncdi.HeatOfHydration.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Linq.Expressions;
using VolPro.Core.BaseProvider;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.Extensions;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Core.Utilities;
using VolPro.Entity.DomainModels;
using VolPro.Entity.DomainModels.Hoh.partial;

namespace Hncdi.HeatOfHydration.Services
{
    public partial class Hoh_ProjectService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHoh_ProjectRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public Hoh_ProjectService(
            IHoh_ProjectRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override WebResponseContent Del(object[] keys, bool delList = true)
        {
            if (keys.Length > 1)
            {
                return WebResponseContent.Instance.Error("每次仅可删除一条数据");
            }
            WebResponseContent webResponse = new WebResponseContent();
            var ids = keys.Select(s => s.ToString().GetLong()).ToList()[0];
            bool b = repository.DbContext.Set<Hoh_Project_Point>().Where(x => x.HohProject_id == ids).ToList().Count > 0;
            if (b)
            {
                return WebResponseContent.Instance.Error("该位置下存在测点，不能删除");
            }
            return base.Del(keys, delList);
        }

        public WebResponseContent GetProjectDataInfo(long hohProjectCode)
        {
            WebResponseContent webResponse = new WebResponseContent();
            HohMonitoringData hohMonitoringData = new HohMonitoringData();
            try {
                //取位置信息
                var hoh_Project = repository.DbContext.Set<Hoh_Project>().Where(x => x.HohProject_id == hohProjectCode).FirstOrDefault();
                //取项目信息
                var hncdi_Project = repository.DbContext.Set<Hncdi_Project>().Where(x => x.Project_id == hoh_Project.Project_id).FirstOrDefault();
                List<Hoh_Project_Point> listPoint= repository.DbContext.Set<Hoh_Project_Point>().Where(x => x.Project_id == hoh_Project.Project_id && x.HohProject_id == hoh_Project.HohProject_id).ToList();
                //取项监测数据
                List<V_Hoh_Project_Data> listPoinData = repository.DbContext.Set<V_Hoh_Project_Data>().Where(x => x.Project_id == hoh_Project.Project_id && x.HohProject_id == hoh_Project.HohProject_id).ToList();
                //取报告信息
                List<Hoh_Project_Report> listReport = repository.DbContext.Set<Hoh_Project_Report>().Where(x => x.Project_id == hoh_Project.Project_id && x.HohProject_id == hoh_Project.HohProject_id).ToList();

                hohMonitoringData.title = hncdi_Project.HohName;
                hohMonitoringData.info = hoh_Project.Remark;
                hohMonitoringData.pileCap3D = hoh_Project.PileCap3DImg;
                hohMonitoringData.measurementPointsLayout = hoh_Project.PointsLayout;
                hohMonitoringData.renderingDisplay = hncdi_Project.ProjectImg;
                hohMonitoringData.pouringStartTime = hoh_Project.PourDate.ToString("yyyy-MM-dd HH:mm");
                hohMonitoringData.pouringEndTime = hoh_Project.PourEndDate.ToString("yyyy-MM-dd HH:mm");
                hohMonitoringData.planDuration = (int)Math.Round((hoh_Project.PourEndDate - hoh_Project.PourDate).TotalHours);//计划结束时间 -  开始时间

                hohMonitoringData.coreMeasurementPoints = hoh_Project.CoreMeasurementPoints ?? 0;
                hohMonitoringData.surfaceMeasurementPoints = hoh_Project.SurfaceMeasurementPoints ?? 0;
                hohMonitoringData.waterFlow = string.IsNullOrEmpty(hoh_Project.WaterFlow)
                    ? Array.Empty<float>()
                    : Array.ConvertAll(hoh_Project.WaterFlow.Split(',', '，'), float.Parse);


                hohMonitoringData.temperatureWarning = hoh_Project.warningCount;
                hohMonitoringData.waterWarning = hoh_Project.alarmCount;

                //报表数据
                hohMonitoringData.listReport = listReport.Select(s => new ReportPeriods
                {
                    reportName = s.ReportName,
                    reportDate = s.ReportDate,
                    fileUrl = s.FileUrl
                }).ToList();


                if (listPoinData.Count!=0) {
                    var maxTimeVal = listPoinData.Max(m => m.TimeVal);
                    hohMonitoringData.monitoringDuration = (int)Math.Round((maxTimeVal - hoh_Project.PourDate).TotalHours);//最后数据时间 -  开始时间
                                                                                                                           //报表
                    hohMonitoringData.LastUpdateTime = maxTimeVal.ToString("yyyy-MM-dd HH:mm");//温度曲线数据的最新时间

                    List<PointInfo> pointInfos = listPoinData
                                    .GroupBy(g => new { g.PointId, g.PointName, g.PointType }) // 按测点分组
                                    .Select(g => new PointInfo
                                    {
                                        pointId = g.Key.PointId,
                                        pointName = g.Key.PointName,
                                        pointType = g.Key.PointType,

                                        // 组装 [时间, 测值] 格式
                                        dataList = g.Select(item => new object[]
                                        {
                                    item.TimeVal,  // 时间 DateTime
                                    item.PointVal  // 测值 decimal
                                        }).ToList()
                                    }).ToList();
                    hohMonitoringData.listPointInfo = pointInfos;

                    //大气温度
                    decimal? atmosphericTemperature = listPoinData
                        .Where(x => x.PointType == "5")
                        .GroupBy(x => x.HohProject_id)
                        .FirstOrDefault()?
                        .OrderByDescending(x => x.TimeVal)
                        .First()?
                        .PointVal;
                    //进水口1温度
                    decimal waterIn1Temperature = listPoinData
                        .Where(x => x.PointName.Contains("进水口1"))
                        .GroupBy(x => x.HohProject_id)
                        .FirstOrDefault()?
                        .OrderByDescending(x => x.TimeVal)
                        .First()?
                        .PointVal ?? 0;
                    //出水口1温度
                    decimal waterOut1Temperature = listPoinData
                        .Where(x => x.PointName.Contains("出水口1"))
                        .GroupBy(x => x.HohProject_id)
                        .FirstOrDefault()?
                        .OrderByDescending(x => x.TimeVal)
                        .First()?
                        .PointVal ?? 0;
                    //进水口2温度
                    decimal waterIn2Temperature = listPoinData
                        .Where(x => x.PointName.Contains("进水口2"))
                        .GroupBy(x => x.HohProject_id)
                        .FirstOrDefault()?
                        .OrderByDescending(x => x.TimeVal)
                        .First()?
                        .PointVal ?? 0;
                    //出水口2温度
                    decimal waterOut2Temperature = listPoinData
                        .Where(x => x.PointName.Contains("出水口2"))
                        .GroupBy(x => x.HohProject_id)
                        .FirstOrDefault()?
                        .OrderByDescending(x => x.TimeVal)
                        .First()?
                        .PointVal ?? 0;

                    //入模温度，最大 最小 平均值
                    var (moldmax, moldmin, moldavg) = CalculateMoldingTimeStats(listPoint, listPoinData);
                    //顶表面测点，所有测点最晚一条数据中最大、最小值
                    var (topMax, topMin, topAvg) = CalculateTopSurfaceLatestStats(listPoinData, "3");
                    //水管测点，所有测点最晚一条数据中最大、最小值
                    var (pipMax, pipMin, pipAvg) = CalculateTopSurfaceLatestStats(listPoinData, "2");
                    //核心测点，所有测点最晚一条数据中最大、最小值
                    var (coreMax, coreMin, coreAvg) = CalculateTopSurfaceLatestStats(listPoinData, "1");
                    //核心的 绝对升温，最大最小值
                    var (catMax, catMin) = CalculateTemperatureDifferenceStats(listPoinData, "1", listPoint);
                    //核心区域的最大升温速率
                    string maxRiseRate = CalculateMaxTemperatureRiseRate(listPoinData);
                    //核心区域的最大降温速率（升温速率的-1倍）
                    string maxDropRate = "--";
                    if (maxRiseRate != "--")
                    {
                        if (decimal.TryParse(maxRiseRate, out decimal rate))
                        {
                            decimal dropRate = rate * -1;
                            maxDropRate = dropRate.ToString("f2");
                        }
                    }
                    TMData tMDate = new TMData();
                    tMDate.atmosphericTemperature = atmosphericTemperature.ToString("f1") ?? "--" + "℃";
                    tMDate.moldTemperature = moldmin + "℃ ～ " + moldmax + "℃";
                    tMDate.waterInTemperature = waterIn1Temperature.ToString("f1") + "℃";
                    tMDate.waterOutTemperature = waterOut1Temperature.ToString("f1") + "℃";
                    tMDate.topSurfaceTemperature = topMin + "℃ ～ " + topMax + "℃";
                    tMDate.internalPipeTemperature = pipMin + "℃ ～ " + pipMax + "℃";
                    tMDate.concreteAbsoluteTemperatureRise = catMin + "℃ ～ " + catMax + "℃";
                    tMDate.concreteCoreTemperature = coreMin + "℃ ～ " + coreMax + "℃";
                    hohMonitoringData.tMDate = tMDate;

                    TCData tCData = new TCData();
                    tCData.pipe1TemperatureDifference = Math.Abs(waterIn1Temperature - waterOut1Temperature).ToString("f1") + "℃";
                    tCData.pipe2TemperatureDifference = Math.Abs(waterIn2Temperature - waterOut2Temperature).ToString("f1") + "℃";
                    tCData.surfaceAirTemperatureDifference = (topAvg - atmosphericTemperature).ToString("f1") ?? "--" + "1℃";
                    tCData.surfaceCoreTemperatureDifference = (coreAvg - topAvg).ToString("f1") ?? "--" + "1℃";
                    tCData.temperatureRiseRate = maxRiseRate + "℃/h";
                    tCData.temperatureDropRate = maxDropRate + "℃/h";
                    hohMonitoringData.tCData = tCData;
                }

                return webResponse.OKData(hohMonitoringData);
            } catch (Exception e) { 
                return webResponse.Error("取数异常："+e.Message);
            }

        }

        /// <summary>
        /// 计算入模时间点对应的测值统计数据
        /// </summary>
        /// <param name="listPoint">监控点位列表</param>
        /// <param name="listPoinData">监测数据集合</param>
        /// <returns>包含所有符合条件数据的最大值、最小值和平均值的统计结果</returns>
        public (string MaxValue, string MinValue, string AvgValue) CalculateMoldingTimeStats(List<Hoh_Project_Point> listPoint, List<V_Hoh_Project_Data> listPoinData)
        {
            // 检查参数是否为空
            if (listPoint == null || !listPoint.Any() || listPoinData == null || !listPoinData.Any())
            {
                return ("--", "--", "--");
            }

            try
            {
                // 收集所有有效的(PointId, MoldingTime)对
                var validPoints = listPoint
                    .Where(point => point.PointId > 0 && point.MoldingTime.HasValue)
                    .Select(point => new { point.PointId, MoldingTime=point.MoldingTime.Value })
                    .ToList();

                if (!validPoints.Any())
                {
                    return ("--", "--", "--");
                }

                // 匹配所有符合条件的数据项
                var matchedData = listPoinData
                    .Where(item => validPoints.Any(p => p.PointId == item.PointId && p.MoldingTime == item.TimeVal))
                    .ToList();

                if (!matchedData.Any())
                {
                    return ("--", "--", "--");
                }

                // 计算统计值
                decimal maxValue = matchedData.Max(item => item.PointVal);
                decimal minValue = matchedData.Min(item => item.PointVal);
                decimal avgValue = matchedData.Average(item => item.PointVal);

                return (maxValue.ToString("f1"), minValue.ToString("f1"), avgValue.ToString("f1"));
            }
            catch (Exception)
            {
                // 处理异常情况
                return ("--", "--", "--");
            }
        }

        /// <summary>
        /// 所有测点最晚一条数据中的最大值和最小值
        /// </summary>
        /// <param name="listPoinData">监测数据集合</param>
        /// <returns>包含最大值和最小值的统计结果</returns>
        public (string MaxValue, string MinValue, decimal AvgValue) CalculateTopSurfaceLatestStats(List<V_Hoh_Project_Data> listPoinData,string type)
        {
            // 检查参数是否为空
            if (listPoinData == null || !listPoinData.Any())
            {
                return ("--", "--",0);
            }

            try
            {
                // 筛选顶表面测点（PointType == "3"）
                var topSurfaceData = listPoinData.Where(x => x.PointType == type).ToList();
                if (!topSurfaceData.Any())
                {
                    return ("--", "--", 0);
                }

                // 对每个测点，找到其最晚一条数据
                var latestDataPerPoint = topSurfaceData
                    .GroupBy(x => x.PointId)
                    .Select(g => g.OrderByDescending(x => x.TimeVal).First())
                    .ToList();

                if (!latestDataPerPoint.Any())
                {
                    return ("--", "--", 0);
                }

                // 计算最大值和最小值
                decimal maxValue = latestDataPerPoint.Max(item => item.PointVal);
                decimal minValue = latestDataPerPoint.Min(item => item.PointVal);
                decimal avgValue = latestDataPerPoint.Average(item => item.PointVal);

                return (maxValue.ToString("f1"), minValue.ToString("f1"), avgValue);
            }
            catch (Exception)
            {
                // 处理异常情况
                return ("--", "--", 0);
            }
        }

        /// <summary>
        /// 计算各个测点的历史最高温度与入模温度的差值，并返回所有测点差值的最大、最小值
        /// </summary>
        /// <param name="listPoinData">监测数据集合</param>
        /// <param name="type">测点类型</param>
        /// <param name="listPoint">监控点位列表</param>
        /// <returns>包含所有测点差值的最大、最小值的统计结果</returns>
        public (string MaxDifference, string MinDifference) CalculateTemperatureDifferenceStats(List<V_Hoh_Project_Data> listPoinData, string type, List<Hoh_Project_Point> listPoint)
        {
            // 检查参数是否为空
            if (listPoinData == null || !listPoinData.Any() || string.IsNullOrEmpty(type) || listPoint == null || !listPoint.Any())
            {
                return ("--", "--");
            }

            try
            {
                // 筛选出指定类型的监测数据
                var filteredData = listPoinData.Where(x => x.PointType == type).ToList();
                if (!filteredData.Any())
                {
                    return ("--", "--");
                }

                // 收集有效的测点信息（有入模时间的测点）
                var validPoints = listPoint
                    .Where(point => point.PointId > 0 && point.MoldingTime.HasValue)
                    .Select(point => new { point.PointId, MoldingTime = point.MoldingTime.Value })
                    .ToList();

                if (!validPoints.Any())
                {
                    return ("--", "--");
                }

                // 计算每个测点的差值
                var differences = new List<decimal>();

                foreach (var point in validPoints)
                {
                    // 筛选该测点的历史数据
                    var pointData = filteredData.Where(x => x.PointId == point.PointId).ToList();
                    if (!pointData.Any())
                    {
                        continue;
                    }

                    // 找到历史最高温度
                    decimal maxTemp = pointData.Max(x => x.PointVal);

                    // 找到入模温度（在入模时间点的数据）
                    var moldingData = pointData.Where(x => x.TimeVal == point.MoldingTime).FirstOrDefault();
                    if (moldingData == null)
                    {
                        continue;
                    }

                    // 计算差值
                    decimal difference = maxTemp - moldingData.PointVal;
                    differences.Add(difference);
                }

                if (!differences.Any())
                {
                    return ("--", "--");
                }

                // 计算差值的最大、最小值
                decimal maxDifference = differences.Max();
                decimal minDifference = differences.Min();

                return (maxDifference.ToString("f1"), minDifference.ToString("f1"));
            }
            catch (Exception)
            {
                // 处理异常情况
                return ("--", "--");
            }
        }

        /// <summary>
        /// 计算核心区域(type=1)的最大升温速率
        /// </summary>
        /// <param name="listPoinData">监测数据集合</param>
        /// <returns>最大升温速率（绝对值最大的那个值）</returns>
        public string CalculateMaxTemperatureRiseRate(List<V_Hoh_Project_Data> listPoinData)
        {
            // 检查参数是否为空
            if (listPoinData == null || !listPoinData.Any())
            {
                return "--";
            }

            try
            {
                // 筛选核心区域(type=1)的测点数据
                var coreData = listPoinData.Where(x => x.PointType == "1").ToList();
                if (!coreData.Any())
                {
                    return "--";
                }

                // 按测点分组
                var pointGroups = coreData.GroupBy(x => x.PointId);
                var rates = new List<decimal>();

                foreach (var group in pointGroups)
                {
                    // 按时间排序
                    var sortedData = group.OrderBy(x => x.TimeVal).ToList();
                    if (sortedData.Count < 2)
                    {
                        continue;
                    }

                    // 最新数据
                    var latestData = sortedData.Last();
                    // 4小时前的时间
                    var fourHoursAgo = latestData.TimeVal.AddHours(-4);

                    // 找到4小时前或最接近4小时前的数据
                    var fourHoursAgoData = sortedData
                        .Where(x => x.TimeVal <= fourHoursAgo)
                        .OrderByDescending(x => x.TimeVal)
                        .FirstOrDefault();

                    if (fourHoursAgoData == null)
                    {
                        continue;
                    }

                    // 计算升温速率：(最新值 - 4小时前的值) / 4
                    decimal rate = (latestData.PointVal - fourHoursAgoData.PointVal) / 4;
                    rates.Add(rate);
                }

                if (!rates.Any())
                {
                    return "--";
                }

                // 找到绝对值最大的升温速率
                var maxRate = rates.OrderBy(x => Math.Abs(x)).Last();

                return maxRate.ToString("f2");
            }
            catch (Exception)
            {
                // 处理异常情况
                return "--";
            }
        }

    }
}
