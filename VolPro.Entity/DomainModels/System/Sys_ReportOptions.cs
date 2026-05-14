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
    [Entity(TableCnName = "报表模板",TableName = "Sys_ReportOptions",DBServer = "SysDbContext")]
    public partial class Sys_ReportOptions:SysEntity
    {
        /// <summary>
       ///
       /// </summary>
       [SugarColumn(IsPrimaryKey = true)]
       [Key]
       [Display(Name ="ReportOptionsId")]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public Guid ReportOptionsId { get; set; }

       /// <summary>
       ///报表名称
       /// </summary>
       [Display(Name ="报表名称")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ReportName { get; set; }

       /// <summary>
       ///报表编码
       /// </summary>
       [Display(Name ="报表编码")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ReportCode { get; set; }

       /// <summary>
       ///所在数据库
       /// </summary>
       [Display(Name ="所在数据库")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string DbService { get; set; }

       /// <summary>
       ///报表类型
       /// </summary>
       [Display(Name ="报表类型")]
       [MaxLength(100)]
       [Column(TypeName="varchar(100)")]
       [Editable(true)]
       public string ReportType { get; set; }

       /// <summary>
       ///父级id
       /// </summary>
       [Display(Name ="父级id")]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       public Guid? ParentId { get; set; }

       /// <summary>
       ///模板文件
       /// </summary>
       [Display(Name ="模板文件")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string FilePath { get; set; }

       /// <summary>
       ///数据源sql
       /// </summary>
       [Display(Name ="数据源sql")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string Options { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="OptionsText")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string OptionsText { get; set; }

       /// <summary>
       ///发布状态
       /// </summary>
       [Display(Name ="发布状态")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? Enable { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="DbServiceId")]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       public Guid? DbServiceId { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="UserId")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? UserId { get; set; }

       /// <summary>
       ///租户id
       /// </summary>
       [Display(Name ="租户id")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string TenancyId { get; set; }

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
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
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
       ///修改人
       /// </summary>
       [Display(Name ="修改人")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string Modifier { get; set; }

       /// <summary>
       ///修改时间
       /// </summary>
       [Display(Name ="修改时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ModifyDate { get; set; }

       
    }
}