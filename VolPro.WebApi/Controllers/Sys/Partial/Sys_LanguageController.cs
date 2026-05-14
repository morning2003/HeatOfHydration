/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Sys_Language",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using VolPro.Entity.DomainModels;
using VolPro.Sys.IServices;

namespace VolPro.Sys.Controllers
{
    public partial class Sys_LanguageController
    {
        private readonly ISys_LanguageService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public Sys_LanguageController(
            ISys_LanguageService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        [Route("createLanguagePack"), HttpGet]
        public IActionResult CreateLanguagePack()
        {
            return Json(Service.CreateLanguagePack());
        }
    }
}
