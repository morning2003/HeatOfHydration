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
    [Entity(TableCnName = "语言设置",TableName = "Sys_Language",DBServer = "SysDbContext")]
    public partial class Sys_Language:SysEntity
    {
        /// <summary>
       ///
       /// </summary>
       [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
       [Key]
       [Display(Name ="Id")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int Id { get; set; }

       /// <summary>
       ///简体中文
       /// </summary>
       [Display(Name ="简体中文")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ZHCN { get; set; }

       /// <summary>
       ///繁体中文
       /// </summary>
       [Display(Name ="繁体中文")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string ZHTW { get; set; }

       /// <summary>
       ///英语
       /// </summary>
       [Display(Name ="英语")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string English { get; set; }

       /// <summary>
       ///法语
       /// </summary>
       [Display(Name ="法语")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string French { get; set; }

       /// <summary>
       ///泰语
       /// </summary>
       [Display(Name ="泰语")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string Thai { get; set; }

       /// <summary>
       ///越南语
       /// </summary>
       [Display(Name ="越南语")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string Vietnamese { get; set; }

       /// <summary>
       ///西班牙语
       /// </summary>
       [Display(Name ="西班牙语")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string Spanish { get; set; }

       /// <summary>
       ///俄罗斯语
       /// </summary>
       [Display(Name ="俄罗斯语")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string Russian { get; set; }

       /// <summary>
       ///阿拉伯语
       /// </summary>
       [Display(Name ="阿拉伯语")]
       [MaxLength(2000)]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string Arabic { get; set; }

       /// <summary>
       ///模块
       /// </summary>
       [Display(Name ="模块")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Module { get; set; }

       /// <summary>
       ///语言包
       /// </summary>
       [Display(Name ="语言包")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? IsPackageContent { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="CreateId")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? CreateId { get; set; }

       /// <summary>
       ///创建人
       /// </summary>
       [Display(Name ="创建人")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
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
       [Display(Name ="ModifyId")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? ModifyId { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="ModifyDate")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ModifyDate { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Modifier")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Modifier { get; set; }

       
    }
}