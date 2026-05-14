/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹FormCollectionObjectController编写
 */
using Microsoft.AspNetCore.Mvc;
using VolPro.Core.Controllers.Basic;
using VolPro.Entity.AttributeManager;
using VolPro.Sys.IServices;
namespace VolPro.Sys.Controllers
{
    [Route("api/FormCollectionObject")]
    [PermissionTable(Name = "FormCollectionObject")]
    public partial class FormCollectionObjectController : ApiBaseController<IFormCollectionObjectService>
    {
        public FormCollectionObjectController(IFormCollectionObjectService service)
        : base(service)
        {
        }
    }
}

