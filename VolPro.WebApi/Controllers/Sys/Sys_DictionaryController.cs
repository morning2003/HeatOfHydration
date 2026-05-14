using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VolPro.Core.Controllers.Basic;
using VolPro.Core.Extensions;
using VolPro.Core.Filters;
using VolPro.Sys.IServices;

namespace VolPro.Sys.Controllers
{
    [Route("api/Sys_Dictionary")]
    public partial class Sys_DictionaryController : ApiBaseController<ISys_DictionaryService>
    {
        public Sys_DictionaryController(ISys_DictionaryService service)
        : base("System", "System", "Sys_Dictionary", service)
        {
        }
    }
}
