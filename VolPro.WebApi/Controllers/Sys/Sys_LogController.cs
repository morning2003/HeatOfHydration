using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VolPro.Core.Controllers.Basic;
using VolPro.Core.DBManager;
using VolPro.Entity.DomainModels;
using VolPro.Sys.IServices;

namespace VolPro.Sys.Controllers
{
    [Route("api/Sys_Log")]
    public partial class Sys_LogController : ApiBaseController<ISys_LogService>
    {
        public Sys_LogController(ISys_LogService service)
        : base("System", "System", "Sys_Log", service)
        {
        }

    }
}
