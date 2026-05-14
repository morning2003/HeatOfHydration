/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹Sys_LanguageController编写
 */
using Microsoft.AspNetCore.Mvc;
using VolPro.Core.Controllers.Basic;
using VolPro.Entity.AttributeManager;
using VolPro.Sys.IServices;
namespace VolPro.Sys.Controllers
{
    [Route("api/Sys_Language")]
    [PermissionTable(Name = "Sys_Language")]
    public partial class Sys_LanguageController : ApiBaseController<ISys_LanguageService>
    {
        public Sys_LanguageController(ISys_LanguageService service)
        : base(service)
        {
        }
    }
}

