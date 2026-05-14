using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolPro.Entity.DomainModels.Hoh.partial
{
    public class HohMonitoringData
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        public string info { get; set; }
        /// <summary>
        /// 承台三维图
        /// </summary>
        public string pileCap3D { get; set; }
        /// <summary>
        /// 测点布置图
        /// </summary>
        public string measurementPointsLayout { get; set; }
        /// <summary>
        /// 效果图
        /// </summary>
        public string renderingDisplay { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public string pouringStartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string pouringEndTime { get; set; }
        /// <summary>
        /// 计划时长
        /// </summary>
        public int planDuration { get; set; }
        /// <summary>
        /// 核心区测点数量
        /// </summary>
        public int coreMeasurementPoints { get; set; }
        /// <summary>
        /// 最近更新时间（温度曲线数据的最新时间）
        /// </summary>
        public string LastUpdateTime { get; set; }
        /// <summary>
        /// 表面测点数量
        /// </summary>
        public int surfaceMeasurementPoints { get; set; }
        /// <summary>
        /// 通水情况
        /// </summary>
        public float[] waterFlow { get; set; } = [60, 75, 65, 80];
        /// <summary>
        /// 温度预警情况
        /// </summary>
        public int temperatureWarning { get; set; }
        /// <summary>
        /// 通水预警情况
        /// </summary>
        public int waterWarning { get; set; }
        /// <summary>
        /// 监测时长 =最后数据时间 -  开始时间
        /// </summary>
        public int monitoringDuration { get; set; }

        public List<ReportPeriods> listReport { get; set; }
        public List<PointInfo> listPointInfo { get; set; }

        public TMData tMDate { get; set; }
        public TCData tCData { get; set; }
    }


    /// <summary>
    /// 报告
    /// </summary>
    public class ReportPeriods
    {
        /// <summary>
        /// 报告名称
        /// </summary>
        public string reportName { get; set; }
        /// <summary>
        /// 文件地址
        /// </summary>
        public string fileUrl { get; set; }
        /// <summary>
        /// 报告时间
        /// </summary>
        public DateTime reportDate { get; set; }
        
    }
    /// <summary>
    /// 监测数据
    /// </summary>
    public class TMData {
        /// <summary>
        /// 大气温度
        /// </summary>
        public string atmosphericTemperature { get; set; }
        /// <summary>
        /// 入模温度
        /// </summary>
        public string moldTemperature { get; set; }
        /// <summary>
        /// 入水温度
        /// </summary>
        public string waterInTemperature { get; set; }
        /// <summary>
        /// 出水温度
        /// </summary>
        public string waterOutTemperature { get; set; }
        /// <summary>
        /// 顶表面温度
        /// </summary>
        public string topSurfaceTemperature { get; set; }
        /// <summary>
        /// 内部管位温度
        /// </summary>
        public string internalPipeTemperature { get; set; }
        /// <summary>
        /// 混凝土绝对升温
        /// </summary>
        public string concreteAbsoluteTemperatureRise { get; set; }
        /// <summary>
        /// 混凝土核心温度
        /// </summary>
        public string concreteCoreTemperature { get; set; }
    }

    /// <summary>
    /// 温控数据
    /// </summary>
    public class TCData {
        /// <summary>
        /// 水管1进出口温差
        /// </summary>
        public string pipe1TemperatureDifference { get; set; }
        /// <summary>
        /// 水管2进出口温差
        /// </summary>
        public string pipe2TemperatureDifference { get; set; }
        /// <summary>
        /// 表气温差
        /// </summary>
        public string surfaceAirTemperatureDifference { get; set; }
        /// <summary>
        /// 里表温度
        /// </summary>
        public string surfaceCoreTemperatureDifference { get; set; }
        /// <summary>
        /// 升温速率
        /// </summary>
        public string temperatureRiseRate { get; set; }
        /// <summary>
        /// 降温速率
        /// </summary>
        public string temperatureDropRate { get; set; }
    }

    /// <summary>
    /// 温度测点信息
    /// </summary>
    public class PointInfo
    {
        public long pointId { get; set; }
        /// <summary>
        /// 测点名称
        /// </summary>
        public string pointName { get; set; }
        /// <summary>
        /// 测点类型
        /// </summary>
        public string pointType { get; set; }
        /// <summary>
        /// 时间，温度数据
        /// </summary>
        public List<object[]> dataList { get; set; }
    }
}
