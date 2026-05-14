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
    [Entity(TableCnName = "打印管理",TableName = "Sys_PrintOptions",DBServer = "SysDbContext")]
    public partial class Sys_PrintOptions:SysEntity
    {
        /// <summary>
       ///
       /// </summary>
       [SugarColumn(IsPrimaryKey = true)]
       [Key]
       [Display(Name ="PrintOptionsId")]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public Guid PrintOptionsId { get; set; }

       /// <summary>
       ///模板名称
       /// </summary>
       [Display(Name ="模板名称")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string CustomName { get; set; }

       /// <summary>
       ///打印模块
       /// </summary>
       [Display(Name ="打印模块")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string TableName { get; set; }

       /// <summary>
       ///打印模块名称
       /// </summary>
       [Display(Name ="打印模块名称")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string TableCNName { get; set; }

       /// <summary>
       ///参数
       /// </summary>
       [Display(Name ="参数")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string Options { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Html")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string Html { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Enable")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? Enable { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="DbService")]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       public Guid? DbService { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="UserId")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? UserId { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="TenancyId")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string TenancyId { get; set; }

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
       [MaxLength(30)]
       [Column(TypeName="nvarchar(30)")]
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
       ///
       /// </summary>
       [Display(Name ="DbServiceId")]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       public Guid? DbServiceId { get; set; }

       
    }
}