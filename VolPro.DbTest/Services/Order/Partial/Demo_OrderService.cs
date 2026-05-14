/*
 *所有关于Demo_Order类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*Demo_OrderService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using System.Diagnostics.CodeAnalysis;
using SqlSugar;
using VolPro.Core.DbSqlSugar;

namespace VolPro.DbTest.Services
{
    public partial class Demo_OrderService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDemo_OrderRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public Demo_OrderService(
            IDemo_OrderRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override PageGridData<Demo_Order> GetPageData(PageDataOptions options)
        {
            QueryRelativeExpression = (ISugarQueryable<Demo_Order> queryable) =>
            {
                if (options.Value != null)
                {
                    switch (options.Value.GetInt())
                    {
                        //新订单
                        case 1:
                            queryable = queryable = queryable.Where(c => c.OrderType == 1);
                            break;
                        //采购订单
                        case 2:
                            queryable = queryable = queryable.Where(c => c.OrderType == 2);
                            break;
                        //退货订单
                        case 3:
                            queryable = queryable = queryable.Where(c => c.OrderType == 3);
                            break;
                        default:
                            break;
                    }
                }
                // queryable = queryable = queryable.Where(c => c.OrderType == options.Value.GetInt());

                //当前用户只能操作自己(与下级角色)创建的数据,如:查询、删除、修改等操作
                //IQueryable<int> userQuery = RoleContext.GetCurrentAllChildUser();
                //queryable = queryable.Where(x => x.CreateID == UserContext.Current.UserId || userQuery.Contains(x.CreateID ?? 0));
                return queryable;
            };


            //查询table界面显示求和
            SummaryExpress = (ISugarQueryable<Demo_Order> queryable) =>
            {
                //求和这里不需要group
                return queryable.Select(x => new
                {
                    //注意大小写和数据库字段大小写一样
                    TotalPrice =SqlFunc.AggregateSum(x.TotalPrice),
                    TotalQty = SqlFunc.AggregateSum(x.TotalQty)
                })
                .FirstOrDefault();
            };

            return base.GetPageData(options);
        }

        /// <summary>
        /// 设置弹出框明细表的合计信息
        /// </summary>
        /// <typeparam name="Detail"></typeparam>
        /// <param name="queryeable"></param>
        /// <returns></returns> 
        protected override object GetDetailSummary<Detail>(ISugarQueryable<Detail> queryeable)
        {
            return ((ISugarQueryable<Demo_OrderList>)queryeable).Select(x => new
            {
                //Weight/Qty注意大小写和数据库字段大小写一样
                Price = SqlFunc.AggregateAvg(x.Price),
                Qty = SqlFunc.AggregateSum(x.Qty)
            }).FirstOrDefault();
        }

        public override WebResponseContent Update(SaveModel saveModel)
        {
            return base.Update(saveModel);
        }

        public override WebResponseContent Add(SaveModel saveDataModel)
        {
            saveDataModel.MainData["OrderNo"] = "111";

            WebResponseContent webResponse = new WebResponseContent();
            // 在保存数据库前的操作，所有数据都验证通过了，这一步执行完就执行数据库保存
            AddOnExecuting = (Demo_Order order, object list) =>
            {
               // order.OrderNo = GetOrderNo();
                return webResponse.OK();
            };

            return base.Add(saveDataModel);
        }

        /// <summary>
        /// 自动生成订单号
        /// </summary>
        /// <returns></returns>
        public string GetOrderNo()
        {
            //lock/redis自增
            DateTime dateNow = (DateTime)DateTime.Now.ToString("yyyy-MM-dd").GetDateTime();
            //查询当天最新的订单号
            string orderNo = repository.FindAsIQueryable(x => x.CreateDate > dateNow)
                .OrderByDescending(x => x.OrderNo)
                .Select(s => s.OrderNo)
                .FirstOrDefault();
            string rule = $"D{DateTime.Now.ToString("yyyyMMdd")}";
            if (string.IsNullOrEmpty(orderNo))
            {
                rule += "00001";
            }
            else
            {
                rule += (orderNo.Substring(orderNo.Length - 5).GetInt() + 1).ToString("00000");
            }
            return rule;
        }

    }
}

