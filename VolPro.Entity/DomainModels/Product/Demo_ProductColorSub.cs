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
    [Entity(TableCnName = "产品颜色明细",TableName = "Demo_ProductColorSub",DBServer = "SysDbContext")]
    public partial class Demo_ProductColorSub:SysEntity
    {
        /// <summary>
       ///
       /// </summary>
       [SugarColumn(IsPrimaryKey = true)]
       [Key]
       [Display(Name ="ProductColorSubId")]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public Guid ProductColorSubId { get; set; }

       /// <summary>
       ///产品颜色Id
       /// </summary>
       [Display(Name ="产品颜色Id")]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       public Guid? ProductColorId { get; set; }

       /// <summary>
       ///生产地
       /// </summary>
       [Display(Name ="生产地")]
       [MaxLength(255)]
       [Column(TypeName="varchar(255)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string FactoryLocation { get; set; }

       /// <summary>
       ///生产商
       /// </summary>
       [Display(Name ="生产商")]
       [MaxLength(255)]
       [Column(TypeName="varchar(255)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string Manufacturer { get; set; }

       /// <summary>
       ///生产日期
       /// </summary>
       [Display(Name ="生产日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ProductionDate { get; set; }

       /// <summary>
       ///数量
       /// </summary>
       [Display(Name ="数量")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int Quantity { get; set; }

       /// <summary>
       ///价格
       /// </summary>
       [Display(Name ="价格")]
       [DisplayFormat(DataFormatString="10,2")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public decimal Price { get; set; }

       /// <summary>
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string Remark { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="CreateID")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? CreateID { get; set; }

       /// <summary>
       ///创建人
       /// </summary>
       [Display(Name ="创建人")]
       [MaxLength(30)]
       [Column(TypeName="nvarchar(30)")]
       [Editable(true)]
       public string Creator { get; set; }

       /// <summary>
       ///创建时间
       /// </summary>
       [Display(Name ="创建时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? CreateDate { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="ModifyID")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? ModifyID { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Modifier")]
       [MaxLength(30)]
       [Column(TypeName="nvarchar(30)")]
       [Editable(true)]
       public string Modifier { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="ModifyDate")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ModifyDate { get; set; }

       
    }
}