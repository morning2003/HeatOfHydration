using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VolPro.Core.Controllers.Basic;
using VolPro.Core.Enums;
using VolPro.Core.Filters;
using VolPro.Entity.AttributeManager;
using VolPro.Entity.DomainModels;
using VolPro.Sys.IServices;

namespace VolPro.Sys.Controllers
{
    [Route("api/Sys_Role")]
    [PermissionTable(Name = "Sys_Role")]
    public partial class Sys_RoleController : ApiBaseController<ISys_RoleService>
    {
        public Sys_RoleController(ISys_RoleService service)
        : base("System", "System", "Sys_Role", service)
        {

        }
    }
}


