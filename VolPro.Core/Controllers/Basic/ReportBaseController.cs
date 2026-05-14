using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.DBManager;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.Extensions;
using VolPro.Entity.DomainModels;

namespace VolPro.Core.Controllers.Basic
{
    public class ReportBaseController : VolController
    {
        private ReportOption _reportOptions = null;
        protected ISqlSugarClient DbContext { get; set; }
        protected ReportOption ReportOptions
        {
            get
            {
                if (_reportOptions == null)
                {
                    string code = HttpContext.Request.Query["code"];
                    _reportOptions = DBServerProvider.DbContext.Set<Sys_ReportOptions>().Where(x => x.ReportCode == code)
                                    .Select(s => new ReportOption()
                                    {
                                        ReportOptionsId = s.ReportOptionsId,
                                        ReportCode = s.ReportCode,
                                        DbService = s.DbService,
                                        Sql = s.Options,
                                        ParentId = s.ParentId,
                                        ReportName = s.ReportName,
                                        ReportType = s.ReportType,
                                        FilePath = s.FilePath,

                                    }).ToList().FirstOrDefault();
                    if (_reportOptions == null)
                    {
                        Console.Write($"模板[{code}]不存在");
                    }
                    else
                    {
                        DbContext = DbManger.GetConnection(_reportOptions.DbService);
                    }
                }
                return _reportOptions;
            }
        }
        public ReportBaseController()
        {

        }
        protected object Data = null;
        [HttpGet, HttpPost, Route("getTemplateData")]
        public virtual IActionResult GetTemplateData(string code)
        {
            if (ReportOptions == null)
            {
                return Error("模板不存在");
            }
            string filePath = ReportOptions.FilePath.MapPath(false);
            string text = System.IO.File.ReadAllText(filePath);

            Data = GetData(code);
            if (Data != null)
            {
                return Success(null, new { text, data = Data });
            }
            if (Data == null && !string.IsNullOrEmpty(ReportOptions.Sql))
            {
                Data = DbContext.Ado.SqlQuery<object>(ReportOptions.Sql);
            }
            return Success(null, new { text, data = new { Table = Data } });
        }

        protected virtual object GetData(string code)
        {
            return null;
        }
        //[HttpGet, Route("getData")]
        //public virtual async Task<IActionResult> GetData(string code)
        //{

        //}
    }

    public class ReportOption
    {

        [Key]
        [Display(Name = "ReportOptionsId")]
        [Column(TypeName = "uniqueidentifier")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public Guid ReportOptionsId { get; set; }

        /// <summary>
        ///报表名称
        /// </summary>
        [Display(Name = "报表名称")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public string ReportName { get; set; }

        /// <summary>
        ///报表编码
        /// </summary>
        [Display(Name = "报表编码")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public string ReportCode { get; set; }

        /// <summary>
        ///所在数据库
        /// </summary>
        [Display(Name = "所在数据库")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [Editable(true)]
        public string DbService { get; set; }

        /// <summary>
        ///报表类型
        /// </summary>
        [Display(Name = "报表类型")]
        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        [Editable(true)]
        public string ReportType { get; set; }

        /// <summary>
        ///父级id
        /// </summary>
        [Display(Name = "父级id")]
        [Column(TypeName = "uniqueidentifier")]
        [Editable(true)]
        public Guid? ParentId { get; set; }

        /// <summary>
        ///模板文件
        /// </summary>
        [Display(Name = "模板文件")]
        [MaxLength(2000)]
        [Column(TypeName = "nvarchar(2000)")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public string FilePath { get; set; }

        /// <summary>
        ///数据源sql
        /// </summary>
        [Display(Name = "数据源sql")]
        [MaxLength(2000)]
        [Column(TypeName = "nvarchar(2000)")]
        [Editable(true)]
        public string Sql { get; set; }


    }
}
