using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Quartz.Impl.AdoJobStore.Common;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.DBManager;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.EFDbContext;
using VolPro.Core.Extensions;
using VolPro.Entity.DomainModels;

namespace VolPro.Core.Print
{
    public class PrintCustom : PrintFilter
    {
        public PrintCustom() { }
        /// <summary>
        /// 主表查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public override ISugarQueryable<T> Query<T>(ISugarQueryable<T> query, PrintQuery parms) where T : class
        {
            //判断是哪张主表自定义过滤条件
            if (typeof(T).Name == typeof(Demo_Order).Name)
            {
                var orderQuery = ((ISugarQueryable<Demo_Order>)query);
                return orderQuery
                       //这里where可以写自定义条件
                       .Where(x => true) as ISugarQueryable<T>;

            }
            return query;
        }
        /// <summary>
        /// 明细表查询
        /// </summary>
        /// <typeparam name="Detail"></typeparam>
        /// <param name="query"></param>
        /// <param name="parms"></param>
        /// <returns></returns>

        public override ISugarQueryable<Detail> QueryDetail<Detail>(ISugarQueryable<Detail> query, PrintQuery parms) where Detail : class
        {
            //判断是哪张明细表自定义过滤条件
            if (typeof(Detail).Name == typeof(Demo_OrderList).Name)
            {
                var orderQuery = ((ISugarQueryable<Demo_OrderList>)query);
                return orderQuery
                       //这里where可以写自定义条件
                       .Where(x => true) as ISugarQueryable<Detail>;

            }
            return query;
        }

        /// <summary>
        /// 对返回的结果自定义处理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Detail"></typeparam>
        /// <param name="result"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public override List<Dictionary<string, object>> QueryResult<T>(
            List<Dictionary<string, object>> result,
            PrintQuery parms,
            BaseDbContext dbContext)
        {
            if (result.Count == 0) return result;

            //判断表，自定义返回数据
            if (typeof(T).Name == typeof(Demo_Order).Name)
            {
                //判断是哪个打印模板，然后自定义返回数据
                if (parms.TemplateName == "订单管理主从明细表打印")
                {
                    //返回DemoOrder表自定义配置
                    SetDemoOrderValue(result, parms, dbContext);
                }
            }
            return result;
        }



        /// <summary>
        /// 返回DemoOrder表自定义配置
        /// </summary>
        /// <param name="result"></param>
        /// <param name="parms"></param>
        /// <param name="dbContext"></param>
        private void SetDemoOrderValue(
            List<Dictionary<string, object>> result,
            PrintQuery parms,
            BaseDbContext dbContext)
        {
            //给明细表设置合计
            var data = dbContext.Set<Demo_OrderList>()
                  //根据主表id查询返回明细表合计
                  .Where(x => x.Order_Id == parms.Ids[0].GetGuid())
                  .Select(s => new
                  {
                      单价合计 =SqlFunc.AggregateSum(s.Price),
                      数量合计 = SqlFunc.AggregateSum(s.Qty),
                  }).FirstOrDefault();

            //设置自定义返回的字段(模板设计页面需要定义：单价合计、数量合计两个字段)
            result[0]["单价合计"] = data?.单价合计 ?? 0;
            result[0]["数量合计"] = data?.数量合计 ?? 0;

            //result[0]这里还可以自定义其他字段设置值与模板设计页面定义的字段一致即可

            ////例如：再返回一些自定义的表格数据
            //var otherTable = dbContext.Set<Demo_Product>()
            //    .Where(x => true)//这里写条件
            //    .Select(s => new
            //    {
            //        名称 = s.ProductName,//[名称]与[编号]是打印板自定义表格的里面输入的字段名
            //        编号 = s.ProductCode
            //    }).ToList();

            //result[0]["字段名"] = otherTable;
        }
    }
}
