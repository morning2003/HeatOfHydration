/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹Demo_ProductColorController编写
 */
using Microsoft.AspNetCore.Mvc;
using VolPro.Core.Controllers.Basic;
using VolPro.Entity.AttributeManager;
using VolPro.DbTest.IServices;
namespace VolPro.DbTest.Controllers
{
    [Route("api/Demo_ProductColor")]
    [PermissionTable(Name = "Demo_ProductColor")]
    public partial class Demo_ProductColorController : ApiBaseController<IDemo_ProductColorService>
    {
        public Demo_ProductColorController(IDemo_ProductColorService service)
        : base(service)
        {
        }
    }
}

