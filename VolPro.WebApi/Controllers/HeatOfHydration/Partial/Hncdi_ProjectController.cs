/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Hncdi_Project",Enums.ActionPermissionOptions.Search)]
 */
using Hncdi.HeatOfHydration.IRepositories;
using Hncdi.HeatOfHydration.IServices;
using Hncdi.HeatOfHydration.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VolPro.Entity.DomainModels;

namespace Hncdi.HeatOfHydration.Controllers
{
    public partial class Hncdi_ProjectController
    {
        private readonly IHncdi_ProjectService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHncdi_ProjectRepository _hncdi_ProjectRepository;

        [ActivatorUtilitiesConstructor]
        public Hncdi_ProjectController(
            IHncdi_ProjectService service,
            IHncdi_ProjectRepository hoh_ProjectRepository,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _hncdi_ProjectRepository = hoh_ProjectRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// 根据项目名称获取水化热监控部位，用户级联选择
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet, Route("getList")]
        public async Task<IActionResult> GetList()
        {
            return Json(await _hncdi_ProjectRepository.FindAsIQueryable(x => 1 == 1)
                  .Select(s => new
                  {
                      key = s.Project_id,
                      value = s.ProjectName
                  }).ToListAsync());
        }
    }
}
