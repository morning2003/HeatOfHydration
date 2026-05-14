/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果数据库字段发生变化，请在代码生器重新生成此Model
 */
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using VolPro.Entity.SystemModels;

namespace VolPro.Entity.DomainModels
{
    [Entity(TableCnName = "用户权限", TableName = "Sys_UserAuth", DBServer = "SysDbContext")]
    public partial class Sys_UserAuth : SysEntity
    {
        /// <summary>
        ///
        /// </summary>
        [Key]
        [Display(Name = "Id")]
        [Column(TypeName = "int")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        ///用户id
        /// </summary>
        [Display(Name = "用户id")]
        [Column(TypeName = "int")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public int UserId { get; set; }

        /// <summary>
        ///指定用户id的数据权限
        /// </summary>
        [Display(Name = "指定用户id的数据权限")]
        [Column(TypeName = "nvarchar(max)")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public string AuthUserIds { get; set; }

        /// <summary>
        ///菜单id
        /// </summary>
        [Display(Name = "菜单id")]
        [Column(TypeName = "int")]
        [Editable(true)]
        public int? MenuId { get; set; }

        /// <summary>
        ///表
        /// </summary>
        [Display(Name = "表")]
        [MaxLength(255)]
        [Column(TypeName = "nvarchar(255)")]
        public string TableName { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Display(Name = "Enable")]
        [Column(TypeName = "int")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public int Enable { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Display(Name = "CreateID")]
        [Column(TypeName = "int")]
        [Editable(true)]
        public int? CreateID { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Display(Name = "Creator")]
        [MaxLength(255)]
        [Column(TypeName = "nvarchar(255)")]
        [Editable(true)]
        public string Creator { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Display(Name = "CreateDate")]
        [Column(TypeName = "datetime")]
        [Editable(true)]
        public DateTime? CreateDate { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Display(Name = "ModifyID")]
        [Column(TypeName = "int")]
        [Editable(true)]
        public int? ModifyID { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Display(Name = "Modifier")]
        [MaxLength(255)]
        [Column(TypeName = "nvarchar(255)")]
        [Editable(true)]
        public string Modifier { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Display(Name = "ModifyDate")]
        [Column(TypeName = "datetime")]
        [Editable(true)]
        public DateTime? ModifyDate { get; set; }


    }
}