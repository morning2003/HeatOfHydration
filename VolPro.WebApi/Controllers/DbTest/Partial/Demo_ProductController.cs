/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Demo_Product",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using VolPro.Entity.DomainModels;
using VolPro.DbTest.IServices;
using VolPro.Core.Filters;

namespace VolPro.DbTest.Controllers
{
    public partial class Demo_ProductController
    {
        private readonly IDemo_ProductService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public Demo_ProductController(
            IDemo_ProductService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }
        [ApiActionPermission()]
        public override ActionResult GetPageData([FromBody] PageDataOptions loadData)
        {
            return base.GetPageData(loadData);
        }
        [ApiActionPermission()]
        public override ActionResult Add([FromBody] SaveModel saveModel)
        {
            return base.Add(saveModel);
        }
        [ApiActionPermission()]
        public override ActionResult Update([FromBody] SaveModel saveModel)
        {
            return base.Update(saveModel);
        }
        [ApiActionPermission()]
        public override ActionResult Del([FromBody] object[] keys)
        {
            return base.Del(keys);
        }
    }
}
