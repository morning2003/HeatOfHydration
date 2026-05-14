/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Sys_Dashboard",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using VolPro.Entity.DomainModels;
using VolPro.Sys.IServices;
using VolPro.Core.Filters;
using VolPro.Core.Enums;

namespace VolPro.Sys.Controllers
{
    public partial class Sys_DashboardController
    {
        private readonly ISys_DashboardService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public Sys_DashboardController(
            ISys_DashboardService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// 编译、预览、查看获取全部配置
        /// </summary>
        /// <param name="id"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        [HttpGet, HttpPost, Route("getLayoutData")]
        public async Task<IActionResult> GetLayoutData(Guid id, bool view)
        {
            return Json(await Service.GetLayoutData(id, view));
        }
        /// <summary>
        ///  获取每项自定义sql数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        [HttpPost, Route("getItemData")]
        public async Task<IActionResult> GetItemData(Guid id, string itemId, DateTime? date1, DateTime? date2, string filterType)
        {
            return Json(await Service.GetItemData(id, itemId, date1, date2, filterType));
        }


        /// <summary>
        /// 编译执行sql
        /// </summary>
        /// <param name="id"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        [HttpPost, Route("execSql")]
        [ApiActionPermission(ActionPermissionOptions.Add | ActionPermissionOptions.Update)]
        public async Task<IActionResult> ExecSql([FromBody] Dictionary<string, string> dic)
        {
            return Json(await Service.ExecSql(dic));
        }
    }
}
