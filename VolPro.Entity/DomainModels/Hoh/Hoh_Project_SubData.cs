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
    [Entity(TableCnName = "监控数据",TableName = "Hoh_Project_SubData",DBServer = "ServiceDbContext")]
    public partial class Hoh_Project_SubData:ServiceEntity
    {
        /// <summary>
       ///
       /// </summary>
       [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
       [Key]
       [Display(Name ="Monitor_id")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long Monitor_id { get; set; }

       /// <summary>
       ///监测部位
       /// </summary>
       [Display(Name ="监测部位")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       public long? HohProject_id { get; set; }

       /// <summary>
       ///监测点位
       /// </summary>
       [Display(Name ="监测点位")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long PointId { get; set; }

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
       ///数据时间
       /// </summary>
       [Display(Name ="数据时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public DateTime TimeVal { get; set; }

       /// <summary>
       ///创建时间
       /// </summary>
       [Display(Name ="创建时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? CreateDate { get; set; }

       
    }
}