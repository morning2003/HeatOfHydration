/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Demo_Catalog",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using VolPro.Entity.DomainModels;
using VolPro.DbTest.IServices;
using VolPro.Core.Enums;
using VolPro.Core.Extensions;
using VolPro.Core.Filters;
using VolPro.DbTest.Repositories;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SqlSugar;

namespace VolPro.DbTest.Controllers
{
    public partial class Demo_CatalogController
    {
        private readonly IDemo_CatalogService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;
        [ActivatorUtilitiesConstructor]
        public Demo_CatalogController(
            IDemo_CatalogService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 商品信息tree页面获取左边的tree的所有商品分类
        /// </summary>
        /// <returns></returns>
        [Route("getList"), HttpGet]
        public async Task<IActionResult> GetList()
        {
            var data = await Demo_CatalogRepository.Instance.FindAsIQueryable(x => true)
                  .Select(s => new
                  {
                      id = s.CatalogId,
                      s.ParentId,
                      name = s.CatalogName
                  })
                  .ToListAsync();
            return Json(data);
        }

        public override ActionResult GetPageData([FromBody] PageDataOptions loadData)
        {
            //没有查询条件显示所有一级节点数据
            if (loadData.Value.GetInt() == 1)
            {
                return GetCatalogRootData(loadData);
            }
            //有查询条件使用框架默认的查询方法
            return base.GetPageData(loadData);
        }

        /// treetable 获取根节点数据
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("getCatalogRootData")]
        [ApiActionPermission(ActionPermissionOptions.Search)]
        public ActionResult GetCatalogRootData([FromBody] PageDataOptions options)
        {
            //页面加载(一级)根节点数据条件x => x.ParentId==null,自己根据需要设置
            var query = Demo_CatalogRepository.Instance.FindAsIQueryable(x => x.ParentId == null);

            var rows = query.TakeOrderByPage(options.Page, options.Rows)
                .OrderBy(x => x.CatalogName).Select(s => new
                {
                    s.CatalogId,
                    s.CatalogName,
                    s.CatalogCode,
                    s.ParentId,
                    s.Img,
                    s.Enable,
                    s.Remark,
                    s.CreateID,
                    s.Creator,
                    s.CreateDate,
                    s.ModifyID,
                    s.Modifier,
                    s.ModifyDate,
                    hasChildren = true
                }).ToList();
            return JsonNormal(new { total = query.Count(), rows });
        }


        /// <summary>
        ///treetable 获取子节点数据
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("getChildrenData")]
        [ApiActionPermission(ActionPermissionOptions.Search)]
        public async Task<ActionResult> GetChildrenData(Guid catalogId)
        {
            //点击节点时，加载子节点数据
            var roleRepository = Demo_CatalogRepository.Instance.FindAsIQueryable(x => 1 == 1);

            var rows = await roleRepository.Where(x => x.ParentId == catalogId)
                .Select(s => new
                {
                    s.CatalogId,
                    s.CatalogName,
                    s.CatalogCode,
                    s.ParentId,
                    s.Img,
                    s.Enable,
                    s.Remark,
                    s.CreateID,
                    s.Creator,
                    s.CreateDate,
                    s.ModifyID,
                    s.Modifier,
                    s.ModifyDate,
                    hasChildren = SqlFunc.Subqueryable<Demo_Catalog>().Where(x => x.ParentId == s.CatalogId).Any()
                }).ToListAsync();
            return JsonNormal(new { rows });
        }
    }
}
