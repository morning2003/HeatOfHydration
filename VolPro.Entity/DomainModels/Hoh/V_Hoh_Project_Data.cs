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
    [Entity(TableCnName = "监控数据-视图",TableName = "V_Hoh_Project_Data",DBServer = "ServiceDbContext")]
    public partial class V_Hoh_Project_Data:ServiceEntity
    {
        /// <summary>
       ///项目名称
       /// </summary>
       [Display(Name ="项目名称")]
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
       ///
       /// </summary>
       [Display(Name ="ProjectName")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ProjectName { get; set; }

       /// <summary>
       ///监测部位
       /// </summary>
       [Display(Name ="监测部位")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long HohProject_id { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="SectionName")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string SectionName { get; set; }

       /// <summary>
       ///监测点位
       /// </summary>
       [Display(Name ="监测点位")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string PointName { get; set; }

       /// <summary>
       ///测点类型
       /// </summary>
       [Display(Name ="测点类型")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string PointType { get; set; }

       /// <summary>
       ///测值
       /// </summary>
       [Display(Name ="测值")]
       [DisplayFormat(DataFormatString="10,5")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public decimal PointVal { get; set; }

       /// <summary>
       ///时间
       /// </summary>
       [Display(Name ="时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public DateTime TimeVal { get; set; }

       /// <summary>
       ///
       /// </summary>
       [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
       [Key]
       [Display(Name ="ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long ID { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="PointId")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long PointId { get; set; }

       
    }
}