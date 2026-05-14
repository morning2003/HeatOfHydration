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
    [Entity(TableCnName = "基础信息",TableName = "Hncdi_Project",DBServer = "ServiceDbContext")]
    public partial class Hncdi_Project:ServiceEntity
    {
        /// <summary>
       ///项目主键ID
       /// </summary>
       [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
       [Key]
       [Display(Name ="项目主键ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long Project_id { get; set; }

       /// <summary>
       ///项目编号
       /// </summary>
       [Display(Name ="项目编号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ProjectCode { get; set; }

       /// <summary>
       ///项目名称
       /// </summary>
       [Display(Name ="项目名称")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ProjectName { get; set; }

       /// <summary>
       ///项目负责人
       /// </summary>
       [Display(Name ="项目负责人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ProjectCharge { get; set; }

       /// <summary>
       ///工程类型
       /// </summary>
       [Display(Name ="工程类型")]
       [MaxLength(1)]
       [Column(TypeName="char(1)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ProjectType { get; set; }

       /// <summary>
       ///项目状态
       /// </summary>
       [Display(Name ="项目状态")]
       [MaxLength(1)]
       [Column(TypeName="char(1)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ProjectStatus { get; set; }

       /// <summary>
       ///工程地点
       /// </summary>
       [Display(Name ="工程地点")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string ConstructAddress { get; set; }

       /// <summary>
       ///建设单位
       /// </summary>
       [Display(Name ="建设单位")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string BuildUnit { get; set; }

       /// <summary>
       ///监理单位
       /// </summary>
       [Display(Name ="监理单位")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string SuperviseUnit { get; set; }

       /// <summary>
       ///施工单位
       /// </summary>
       [Display(Name ="施工单位")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string ConstructUnit { get; set; }

       /// <summary>
       ///设计单位
       /// </summary>
       [Display(Name ="设计单位")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string DesignUnit { get; set; }

       /// <summary>
       ///监控单位
       /// </summary>
       [Display(Name ="监控单位")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string MonitorUnit { get; set; }

       /// <summary>
       ///开工日期
       /// </summary>
       [Display(Name ="开工日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public DateTime StartDate { get; set; }

       /// <summary>
       ///计划竣工日期
       /// </summary>
       [Display(Name ="计划竣工日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? EndDate { get; set; }

       /// <summary>
       ///水化热大屏标题
       /// </summary>
       [Display(Name ="水化热大屏标题")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string HohName { get; set; }

       /// <summary>
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(16)]
       [Column(TypeName="text(16)")]
       [Editable(true)]
       public string Remark { get; set; }

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
       ///项目图片
       /// </summary>
       [Display(Name ="项目图片")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string ProjectImg { get; set; }

       
    }
}