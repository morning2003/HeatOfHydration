/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹Demo_CustomerController编写
 */
using Microsoft.AspNetCore.Mvc;
using VolPro.Core.Controllers.Basic;
using VolPro.Entity.AttributeManager;
using VolPro.DbTest.IServices;
namespace VolPro.DbTest.Controllers
{
    [Route("api/Demo_Customer")]
    [PermissionTable(Name = "Demo_Customer")]
    public partial class Demo_CustomerController : ApiBaseController<IDemo_CustomerService>
    {
        public Demo_CustomerController(IDemo_CustomerService service)
        : base(service)
        {
        }
    }
}

