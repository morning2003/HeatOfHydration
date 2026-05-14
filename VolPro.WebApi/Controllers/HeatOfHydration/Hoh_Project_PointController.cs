/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹Hoh_Project_PointController编写
 */
using Microsoft.AspNetCore.Mvc;
using VolPro.Core.Controllers.Basic;
using VolPro.Entity.AttributeManager;
using Hncdi.HeatOfHydration.IServices;
namespace Hncdi.HeatOfHydration.Controllers
{
    [Route("api/Hoh_Project_Point")]
    [PermissionTable(Name = "Hoh_Project_Point")]
    public partial class Hoh_Project_PointController : ApiBaseController<IHoh_Project_PointService>
    {
        public Hoh_Project_PointController(IHoh_Project_PointService service)
        : base(service)
        {
        }
    }
}

