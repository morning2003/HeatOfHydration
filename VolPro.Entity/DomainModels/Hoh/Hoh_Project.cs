/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果数据库字段发生变化，请在代码生器重新生成此Model
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using VolPro.Entity.SystemModels;

namespace VolPro.Entity.DomainModels
{
    [Entity(TableCnName = "监控部位",TableName = "Hoh_Project",DetailTableCnName = "测点数据",DBServer = "ServiceDbContext")]
    public partial class Hoh_Project:ServiceEntity
    {
        /// <summary>
       ///水化热子项目ID（主键）
       /// </summary>
       [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
       [Key]
       [Display(Name ="水化热子项目ID（主键）")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long HohProject_id { get; set; }

       /// <summary>
       ///主项目
       /// </summary>
       [Display(Name ="主项目")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long Project_id { get; set; }

       /// <summary>
       ///部位名称
       /// </summary>
       [Display(Name ="部位名称")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string SectionName { get; set; }

       /// <summary>
       ///承台三维图
       /// </summary>
       [Display(Name ="承台三维图")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string PileCap3DImg { get; set; }

       /// <summary>
       ///测点布置图
       /// </summary>
       [Display(Name ="测点布置图")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string PointsLayout { get; set; }

       /// <summary>
       ///浇筑状态
       /// </summary>
       [Display(Name ="浇筑状态")]
       [MaxLength(20)]
       [Column(TypeName="nvarchar(20)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string HohStatus { get; set; }

       /// <summary>
       ///浇筑开始时间
       /// </summary>
       [Display(Name ="浇筑开始时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public DateTime PourDate { get; set; }

       /// <summary>
       ///浇筑结束时间
       /// </summary>
       [Display(Name ="浇筑结束时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public DateTime PourEndDate { get; set; }

       /// <summary>
       ///通水运行情况
       /// </summary>
       [Display(Name ="通水运行情况")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string WaterFlow { get; set; }

       /// <summary>
       ///数据采集起始时间
       /// </summary>
       [Display(Name ="数据采集起始时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? DataStartTime { get; set; }

       /// <summary>
       ///数据采集截止时间
       /// </summary>
       [Display(Name ="数据采集截止时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? DataEndTime { get; set; }

       /// <summary>
       ///混凝土标号
       /// </summary>
       [Display(Name ="混凝土标号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       public string ConcreteGrade { get; set; }

       /// <summary>
       ///浇筑方量(m³)
       /// </summary>
       [Display(Name ="浇筑方量(m³)")]
       [DisplayFormat(DataFormatString="18,2")]
       [Column(TypeName="decimal")]
       public decimal? PourVolume { get; set; }

       /// <summary>
       ///核心区测点数量
       /// </summary>
       [Display(Name ="核心区测点数量")]
       [Column(TypeName="int")]
       public int? CoreMeasurementPoints { get; set; }

       /// <summary>
       ///表面测点数量
       /// </summary>
       [Display(Name ="表面测点数量")]
       [Column(TypeName="int")]
       public int? SurfaceMeasurementPoints { get; set; }

       /// <summary>
       ///预警次数
       /// </summary>
       [Display(Name ="预警次数")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int warningCount { get; set; }

       /// <summary>
       ///报警次数
       /// </summary>
       [Display(Name ="报警次数")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int alarmCount { get; set; }

       /// <summary>
       ///指令次数
       /// </summary>
       [Display(Name ="指令次数")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int instructionCount { get; set; }

       /// <summary>
       ///报表期数
       /// </summary>
       [Display(Name ="报表期数")]
       [Column(TypeName="int")]
       public int? reportPeriods { get; set; }

       /// <summary>
       ///创建时间
       /// </summary>
       [Display(Name ="创建时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? CreateDate { get; set; }

       /// <summary>
       ///创建人ID
       /// </summary>
       [Display(Name ="创建人ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? CreateID { get; set; }

       /// <summary>
       ///创建人
       /// </summary>
       [Display(Name ="创建人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Creator { get; set; }

       /// <summary>
       ///修改人
       /// </summary>
       [Display(Name ="修改人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Modifier { get; set; }

       /// <summary>
       ///修改时间
       /// </summary>
       [Display(Name ="修改时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ModifyDate { get; set; }

       /// <summary>
       ///修改人ID
       /// </summary>
       [Display(Name ="修改人ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? ModifyID { get; set; }

       /// <summary>
       ///介绍
       /// </summary>
       [Display(Name ="介绍")]
       [MaxLength(16)]
       [Column(TypeName="text(16)")]
       [Editable(true)]
       public string Remark { get; set; }

       
    }
}