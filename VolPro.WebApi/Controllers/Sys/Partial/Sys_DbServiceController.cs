/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Sys_DbService",Enums.ActionPermissionOptions.Search)]
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

namespace VolPro.Sys.Controllers
{
    public partial class Sys_DbServiceController
    {
       
        private readonly ISys_DbServiceService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public Sys_DbServiceController(
            ISys_DbServiceService service,
            IHttpContextAccessor httpContextAccessor
        ) 
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }
       // [ApiActionPermission(ActionRolePermission.SuperAdmin )]
        [HttpPost, Route("GetPageData")]
        public override ActionResult GetPageData([FromBody] PageDataOptions loadData)
        {
            return base.GetPageData(loadData);
        }
       // [ApiActionPermission(ActionRolePermission.SuperAdmin )]
        [HttpPost, Route("Update")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public override ActionResult Update([FromBody] SaveModel saveModel)
        {
            return base.Update(saveModel);
        }
       // [ApiActionPermission(ActionRolePermission.SuperAdmin )]
        [HttpPost, Route("Add")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public override ActionResult Add([FromBody] SaveModel saveModel)
        {
            return base.Add(saveModel);
        }
        [ApiActionPermission(ActionRolePermission.SuperAdmin| ActionRolePermission.Admin)]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("Export")]
        public override ActionResult Export([FromBody] PageDataOptions loadData)
        {
            return base.Export(loadData);
        }
      
        [HttpPost, Route("createDb")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiActionPermission(ActionRolePermission.SuperAdmin )]
        public ActionResult CreateDb(Guid id)
        {
          return Json(Service.CreateDb(id));
        }
    }
}
