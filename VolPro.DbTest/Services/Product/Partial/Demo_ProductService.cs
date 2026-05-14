/*
 *所有关于Demo_Product类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*Demo_ProductService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;
using System.Linq;
using VolPro.Core.Utilities;
using System.Linq.Expressions;
using VolPro.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using VolPro.DbTest.IRepositories;
using SqlSugar;
using VolPro.Core.DbSqlSugar;

namespace VolPro.DbTest.Services
{
    public partial class Demo_ProductService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDemo_ProductRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public Demo_ProductService(
            IDemo_ProductRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }


        /// <summary>
        /// 主表设置合计
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public override PageGridData<Demo_Product> GetPageData(PageDataOptions options)
        {
            //查询table界面显示求和
            SummaryExpress = (ISugarQueryable<Demo_Product> queryable) =>
            {
                return queryable.Select(x => new
                {
                    //注意大小写和数据库字段大小写一样
                    Price = SqlFunc.AggregateSum(x.Price)
                })
                .FirstOrDefault();
            };

            return base.GetPageData(options);
        }
    }
}
