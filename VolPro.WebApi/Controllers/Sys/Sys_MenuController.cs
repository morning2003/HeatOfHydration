using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VolPro.Core.Controllers.Basic;
using VolPro.Core.Enums;
using VolPro.Core.Filters;
using VolPro.Entity.DomainModels;
using VolPro.Sys.IServices;

namespace VolPro.Sys.Controllers
{
    [Route("api/menu")]
    [ApiController, JWTAuthorize()]
    public partial class Sys_MenuController : ApiBaseController<ISys_MenuService>
    {
        private ISys_MenuService _service { get; set; }
        public Sys_MenuController(ISys_MenuService service) :
            base("System", "System", "Sys_Menu", service)
        {
            _service = service;
        } 
    }
}
