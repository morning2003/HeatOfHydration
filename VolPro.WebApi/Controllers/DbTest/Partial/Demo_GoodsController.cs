/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Demo_Goods",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using VolPro.Entity.DomainModels;
using VolPro.DbTest.IServices;
using VolPro.DbTest.IRepositories;
using System.Linq;
using VolPro.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace VolPro.DbTest.Controllers
{
    public partial class Demo_GoodsController
    {
        private readonly IDemo_GoodsService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDemo_GoodsRepository _repository;
        [ActivatorUtilitiesConstructor]
        public Demo_GoodsController(
            IDemo_GoodsService service,
            IHttpContextAccessor httpContextAccessor,
            IDemo_GoodsRepository repository
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
            _repository = repository;
        }

        /// <summary>
        /// 订单管理页面的明细表table搜索功能
        /// </summary>
        /// <param name="loadData"></param>
        /// <returns></returns>
        //可以定义接口权限
        //[ApiActionPermission(ActionPermissionOptions.Search)]
        [Route("search"), HttpPost]
        public async Task<IActionResult> Search([FromBody] PageDataOptions loadData)
        {
            //loadData.Value是前端loadBefore方法设置的value值(输入框搜索的值)
            string value = loadData.Value?.ToString()?.Trim();

            //生成多个字段or查询条件
            var query = _repository.WhereIF(!string.IsNullOrEmpty(value), x => x.GoodsName.Contains(value) || x.GoodsCode.Contains(value));

            //返回数据数据必须包括rows与total属性
            var data = new
            {
                rows = await query.OrderByDescending(x => x.GoodsName)
                            .TakePage(loadData.Page, loadData.Rows)
                            //返回的字段注意与前端配置的字段一致
                            .Select(s => new
                            {
                                s.GoodsId,
                                s.GoodsName,
                                s.GoodsCode,
                                s.Price,
                                s.Remark
                            }).ToListAsync(),
                //返回总行数
                total = await query.CountAsync()
            };
            //注意前后端字段配置的大小写一致
            return JsonNormal(data);
        }

        [Route("updateStatus"), HttpGet]
        public IActionResult UpdateStatus(Guid goodsId, int enable)
        {
            Demo_Goods goods = new Demo_Goods()
            {
                GoodsId = goodsId,
                Enable = enable
            };
            _repository.Update(goods, x => new { x.Enable }, true);
            return Content("修改成功");
        }
    }
}
