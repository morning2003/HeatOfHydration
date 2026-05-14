using Newtonsoft.Json;
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
    [Entity(TableCnName = "角色管理",TableName = "Sys_Role",DBServer = "SysDbContext")]
    public partial class Sys_Role:SysEntity
    {
        /// <summary>
       ///角色ID
       /// </summary>
       [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
       [Key]
       [Display(Name ="角色ID")]
       [Column(TypeName="int")]
       [Required(AllowEmptyStrings=false)]
       public int Role_Id { get; set; }

       /// <summary>
       ///父级ID
       /// </summary>
       [Display(Name ="父级ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int ParentId { get; set; }

       /// <summary>
       ///角色名称
       /// </summary>
       [Display(Name ="角色名称")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string RoleName { get; set; }

       /// <summary>
       ///部门ID
       /// </summary>
       [Display(Name ="部门ID")]
       [Column(TypeName="int")]
       public int? Dept_Id { get; set; }

       /// <summary>
       ///部门名称
       /// </summary>
       [Display(Name ="部门名称")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string DeptName { get; set; }

       /// <summary>
       ///数据库权限
       /// </summary>
       [Display(Name ="数据库权限")]
       [MaxLength(400)]
       [Column(TypeName="nvarchar(400)")]
       [Editable(true)]
       public string DatAuth { get; set; }

       /// <summary>
       ///是否启用
       /// </summary>
       [Display(Name ="是否启用")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       public byte? Enable { get; set; }

       /// <summary>
       ///排序
       /// </summary>
       [Display(Name ="排序")]
       [Column(TypeName="int")]
       public int? OrderNo { get; set; }

       /// <summary>
       ///创建人
       /// </summary>
       [Display(Name ="创建人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
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
       ///
       /// </summary>
       [Display(Name ="DeleteBy")]
       [MaxLength(50)]
       [JsonIgnore]       [SugarColumn(NoSerialize = true)]
       [Column(TypeName="nvarchar(50)")]
       public string DeleteBy { get; set; }

       /// <summary>
       ///所属数据库
       /// </summary>
       [Display(Name ="所属数据库")]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       public Guid? DbServiceId { get; set; }

       
    }
}
