/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹V_Hoh_Project_DataController编写
 */
using Microsoft.AspNetCore.Mvc;
using VolPro.Core.Controllers.Basic;
using VolPro.Entity.AttributeManager;
using Hncdi.HeatOfHydration.IServices;
namespace Hncdi.HeatOfHydration.Controllers
{
    [Route("api/V_Hoh_Project_Data")]
    [PermissionTable(Name = "V_Hoh_Project_Data")]
    public partial class V_Hoh_Project_DataController : ApiBaseController<IV_Hoh_Project_DataService>
    {
        public V_Hoh_Project_DataController(IV_Hoh_Project_DataService service)
        : base(service)
        {
        }
    }
}

