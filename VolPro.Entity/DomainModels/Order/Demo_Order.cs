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
    [Entity(TableCnName = "订单管理",TableName = "Demo_Order",DetailTable =  new Type[] { typeof(Demo_OrderList)},DetailTableCnName = "订单明细",DBServer = "SysDbContext")]
    public partial class Demo_Order:SysEntity
    {
        /// <summary>
       ///
       /// </summary>
       [SugarColumn(IsPrimaryKey = true)]
       [Key]
       [Display(Name ="Order_Id")]
       [Column(TypeName="uniqueidentifier")]
       [Required(AllowEmptyStrings=false)] 
       public Guid Order_Id { get; set; }

       /// <summary>
       ///订单编号
       /// </summary>
       [Display(Name ="订单编号")]
       [MaxLength(100)]
       [Column(TypeName="varchar(100)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string OrderNo { get; set; }

       /// <summary>
       ///订单类型
       /// </summary>
       [Display(Name ="订单类型")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int OrderType { get; set; }

       /// <summary>
       ///总价
       /// </summary>
       [Display(Name ="总价")]
       [DisplayFormat(DataFormatString="18,2")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? TotalPrice { get; set; }

       /// <summary>
       ///总数量
       /// </summary>
       [Display(Name ="总数量")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? TotalQty { get; set; }

       /// <summary>
       ///订单日期
       /// </summary>
       [Display(Name ="订单日期")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public DateTime OrderDate { get; set; }

       /// <summary>
       ///客户
       /// </summary>
       [Display(Name ="客户")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? CustomerId { get; set; }

       /// <summary>
       ///客户
       /// </summary>
       [Display(Name ="客户")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string Customer { get; set; }

       /// <summary>
       ///手机
       /// </summary>
       [Display(Name ="手机")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string PhoneNo { get; set; }

       /// <summary>
       ///订单状态
       /// </summary>
       [Display(Name ="订单状态")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int OrderStatus { get; set; }

       /// <summary>
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string Remark { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="CreateID")]
       [Column(TypeName="int")]
       public int? CreateID { get; set; }

       /// <summary>
       ///创建人
       /// </summary>
       [Display(Name ="创建人")]
       [MaxLength(30)]
       [Column(TypeName="nvarchar(30)")]
       public string Creator { get; set; }

       /// <summary>
       ///创建时间
       /// </summary>
       [Display(Name ="创建时间")]
       [Column(TypeName="datetime")]
       public DateTime? CreateDate { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="ModifyID")]
       [Column(TypeName="int")]
       public int? ModifyID { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Modifier")]
       [MaxLength(30)]
       [Column(TypeName="nvarchar(30)")]
       public string Modifier { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="ModifyDate")]
       [Column(TypeName="datetime")]
       public DateTime? ModifyDate { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="AuditId")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? AuditId { get; set; }

       /// <summary>
       ///审核人
       /// </summary>
       [Display(Name ="审核人")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string Auditor { get; set; }

       /// <summary>
       ///审核状态
       /// </summary>
       [Display(Name ="审核状态")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? AuditStatus { get; set; }

       /// <summary>
       ///审核时间
       /// </summary>
       [Display(Name ="审核时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? AuditDate { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="AuditReason")]
       [MaxLength(500)]
       [Column(TypeName="nvarchar(500)")]
       [Editable(true)]
       public string AuditReason { get; set; }

       [Display(Name ="订单明细")]
       [ForeignKey("Order_Id")][Navigate(NavigateType.OneToMany,nameof(Order_Id),nameof(Order_Id))]
       public List<Demo_OrderList> Demo_OrderList { get; set; }


       
    }
}