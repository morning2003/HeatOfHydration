/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹Hoh_Project_ReportController编写
 */
using Microsoft.AspNetCore.Mvc;
using VolPro.Core.Controllers.Basic;
using VolPro.Entity.AttributeManager;
using Hncdi.HeatOfHydration.IServices;
namespace Hncdi.HeatOfHydration.Controllers
{
    [Route("api/Hoh_Project_Report")]
    [PermissionTable(Name = "Hoh_Project_Report")]
    public partial class Hoh_Project_ReportController : ApiBaseController<IHoh_Project_ReportService>
    {
        public Hoh_Project_ReportController(IHoh_Project_ReportService service)
        : base(service)
        {
        }
    }
}

