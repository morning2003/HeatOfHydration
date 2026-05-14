/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Sys_CodeRule",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using VolPro.Entity.DomainModels;
using VolPro.Sys.IServices;
using VolPro.Core.UserManager;
using System.Linq;

namespace VolPro.Sys.Controllers
{
    public partial class Sys_CodeRuleController
    {
        private readonly ISys_CodeRuleService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public Sys_CodeRuleController(
            ISys_CodeRuleService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }
        [HttpPost, Route("getTableInfo")]
        public IActionResult GetTableInfo([FromBody] string[] tables)
        {
            var data = TableColumnContext.Data.Where(x => tables.Contains(x.TableName))
                  .Select(x => x.TableName).Distinct().ToList();
            return Json(data);
        }

        [HttpPost, HttpGet, Route("getFields")]
        public IActionResult GetFields(string table)
        {
            var data = TableColumnContext.Data.Where(x => x.TableName == table)
                  //限制只有字符串字段才能设置编号、日期字段设置排序
                  .Where(x =>  new string[] { "string", "date", "datetime" }.Contains(x.ColumnType?.ToLower()))
                  .Select(x => new { key = x.ColumnName, value = x.ColumnCnName, ColumnType = x.ColumnType.ToLower() })
                  .ToList();
            return Json(data);
        }
    }
}
