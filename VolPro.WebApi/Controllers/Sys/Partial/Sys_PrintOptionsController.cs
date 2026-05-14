/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Sys_PrintOptions",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using VolPro.Entity.DomainModels;
using VolPro.Sys.IServices;
using VolPro.Core.Print;
using VolPro.Sys.IRepositories;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.Configuration;
using VolPro.Core.ManageUser;
using VolPro.Core.Tenancy;

namespace VolPro.Sys.Controllers
{
    public partial class Sys_PrintOptionsController
    {
        private readonly ISys_PrintOptionsService _service;//访问业务代码
        private readonly ISys_PrintOptionsRepository _repository;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public Sys_PrintOptionsController(
            ISys_PrintOptionsService service,
            IHttpContextAccessor httpContextAccessor,
            ISys_PrintOptionsRepository repository
        )
        : base(service)
        {
            _service = service;
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// 获取数据源
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("getSelect")]
        public IActionResult GetSelect()
        {
            var data = PrintContainer.GetSelect();
            return Json(data);
        }
        [HttpGet, Route("getPrintFields")]
        public IActionResult GetPrintFields(string table)
        {
            var data = PrintContainer.GetOptions(table);

            return Json(data);
        }
        /// <summary>
        /// 获取打印模板
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        [HttpGet, Route("getPrintTemplateName")]
        public async Task<IActionResult> GetPrintTemplateName(string table)
        {
            var data = await _repository.FindAsIQueryable(x => x.TableName == table)
                   .WhereIF(AppSetting.UseDynamicShareDB, x => x.DbServiceId == UserContext.CurrentServiceId)
                   .FilterTenancy()
                   .OrderByDescending(x => x.CreateDate)
                   .Select(x => new
                   {
                       id = x.PrintOptionsId,
                       name = x.CustomName
                   }).ToListAsync();
            return Json(data);
        }

        [HttpGet, Route("getPrintDetail")]
        public async Task<IActionResult> GetPrintDetail(Guid id)
        {
            var data = await _repository.FindAsIQueryable(x => x.PrintOptionsId == id)
                  .FirstOrDefaultAsync();
            return Json(data);
        }

        /// <summary>
        /// 获取打印数据
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost, Route("getPrintData")]
        public async Task<IActionResult> GetPrintData([FromBody] PrintQuery query)
        {
            var data = await PrintContainer.GetPrintDataAsync(query);
            return JsonNormal(data);
        }


    }
}
