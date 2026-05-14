/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Hoh_Project",Enums.ActionPermissionOptions.Search)]
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
using VolPro.Core.Extensions;
using VolPro.Core.Utilities;
using VolPro.Entity.DomainModels;
using VolPro.Sys.IRepositories;

namespace Hncdi.HeatOfHydration.Controllers
{
    public partial class Hoh_ProjectController
    {
        private readonly IHoh_ProjectService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHoh_ProjectRepository _hoh_ProjectRepository;

        [ActivatorUtilitiesConstructor]
        public Hoh_ProjectController(
            IHoh_ProjectService service,
            IHoh_ProjectRepository hoh_ProjectRepository,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _hoh_ProjectRepository=hoh_ProjectRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// 根据项目名称获取水化热监控部位，用户级联选择
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet, Route("getList")]
        public async Task<IActionResult> GetList(long code)
        {
            return Json(await _hoh_ProjectRepository.FindAsIQueryable(x => x.Project_id == code)
                  .Select(s => new
                  {
                      key = s.HohProject_id,
                      value = s.SectionName
                  }).ToListAsync());
        }
        /// <summary>
        /// 获取大屏数据
        /// </summary>
        /// <param name="code">部位ID</param>
        /// <returns></returns>
        [HttpGet, Route("getProjectDataInfo")]
        public WebResponseContent GetProjectDataInfo(long code)
        {
            WebResponseContent webResponseContent = new WebResponseContent();
            //获取数据
            return Service.GetProjectDataInfo(code);
        }




    }
}
