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
    [Entity(TableCnName = "单据编码",TableName = "Sys_CodeRule",DBServer = "SysDbContext")]
    public partial class Sys_CodeRule:SysEntity
    {
        /// <summary>
       ///
       /// </summary>
       [SugarColumn(IsPrimaryKey = true)]
       [Key]
       [Display(Name ="CodeRuleId")]
       [MaxLength(36)]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public Guid CodeRuleId { get; set; }

       /// <summary>
       ///业务名称
       /// </summary>
       [Display(Name ="业务名称")]
       [MaxLength(255)]
       [Column(TypeName="nvarchar(255)")]
       [Editable(true)]
       public string TableName { get; set; }

       /// <summary>
       ///业务名称
       /// </summary>
       [Display(Name ="业务名称")]
       [MaxLength(255)]
       [Column(TypeName="nvarchar(255)")]
       [Editable(true)]
       public string TableCNName { get; set; }

       /// <summary>
       ///字段
       /// </summary>
       [Display(Name ="字段")]
       [MaxLength(255)]
       [Column(TypeName="nvarchar(255)")]
       [Editable(true)]
       public string Field { get; set; }

       /// <summary>
       ///规则名称
       /// </summary>
       [Display(Name ="规则名称")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string Name { get; set; }

       /// <summary>
       ///编码预览
       /// </summary>
       [Display(Name ="编码预览")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string Code { get; set; }

       /// <summary>
       ///前缀
       /// </summary>
       [Display(Name ="前缀")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string PrefixCode { get; set; }

       /// <summary>
       ///规则类型
       /// </summary>
       [Display(Name ="规则类型")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string RuleType { get; set; }

       /// <summary>
       ///编号位数
       /// </summary>
       [Display(Name ="编号位数")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int ValueLen { get; set; }

       /// <summary>
       ///增长类型
       /// </summary>
       [Display(Name ="增长类型")]
       [MaxLength(1000)]
       [Column(TypeName="nvarchar(1000)")]
       [Editable(true)]
       public string RuleIncremental { get; set; }

       /// <summary>
       ///排序字段
       /// </summary>
       [Display(Name ="排序字段")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string OrderFiled { get; set; }

       /// <summary>
       ///连接符号
       /// </summary>
       [Display(Name ="连接符号")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string ConcatenationSymbol { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="CodeText1")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string CodeText1 { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="CodeText2")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string CodeText2 { get; set; }

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
       [MaxLength(36)]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       public Guid? DbServiceId { get; set; }

       
    }
}