/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹Hncdi_ProjectController编写
 */
using Microsoft.AspNetCore.Mvc;
using VolPro.Core.Controllers.Basic;
using VolPro.Entity.AttributeManager;
using Hncdi.HeatOfHydration.IServices;
namespace Hncdi.HeatOfHydration.Controllers
{
    [Route("api/Hncdi_Project")]
    [PermissionTable(Name = "Hncdi_Project")]
    public partial class Hncdi_ProjectController : ApiBaseController<IHncdi_ProjectService>
    {
        public Hncdi_ProjectController(IHncdi_ProjectService service)
        : base(service)
        {
        }
    }
}

