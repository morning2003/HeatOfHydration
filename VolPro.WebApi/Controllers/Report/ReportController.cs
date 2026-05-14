using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VolPro.Core.Controllers.Basic;
using VolPro.Sys.IRepositories;
using System.Linq;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.Extensions;
using System.IO;

namespace VolPro.WebApi.Controllers
{
    [Route("api/report")]
    public class ReportController : ReportBaseController
    {
        private ISys_ReportOptionsRepository _optionsRepository { get; set; }
        public ReportController(ISys_ReportOptionsRepository optionsRepository)
        {
            _optionsRepository = optionsRepository;
        }

        public override IActionResult GetTemplateData(string code)
        {
            //base.DbContext;
            //base.ReportOptions;
            return base.GetTemplateData(code);
        }
        /// <summary>
        /// 根据报表code自定义返回数据
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        protected override object GetData(string code)
        {
            object data = null;
            //这里只是测试直接写死的数据，实际数据源请在报表模板页面中维护SQL或者使用DbContext自定义查询 
            switch (code)
            {
                case "16700053455897600":
                    data = Get16700053455897600();

                    break;
                case "16700431661089792":
                    data = Get16700431661089792();
                    break;
                default:
                    break;
            }
            return data;
        }

        private object Get16700053455897600()
        {
            return new
            {
                Table = new[]
                                 {
                                new
                                {
                                    OrderID = 10248,
                                    CustomerId = "VINET",
                                    CompanyName = "Vins et aliments",
                                    OrderDate = "1996/7/4 0:00:00",
                                    Freight = 32,
                                    ProductID = 11,
                                    ProductName = "Chai",
                                    UnitPrice = 14,
                                    Quantity = 12,
                                    Discount = 0,
                                    Amount = 168,
                                    DiscountAmt = 0,
                                    NetAmount = 168
                                },
                                new
                                {
                                    OrderID = 10248,
                                    CustomerId = "VINET",
                                    CompanyName = "Vins et aliments",
                                    OrderDate = "1996/7/4 0:00:00",
                                    Freight = 32,
                                    ProductID = 42,
                                    ProductName = "Singaporean Hokkien Fried Mee",
                                    UnitPrice = 10,
                                    Quantity = 10,
                                    Discount = 0,
                                    Amount = 100,
                                    DiscountAmt = 0,
                                    NetAmount = 100
                                },
                                new
                                {
                                    OrderID = 10249,
                                    CustomerId = "TOMSP",
                                    CompanyName = "Toms Spezialit?ten",
                                    OrderDate = "1996/7/5 0:00:00",
                                    Freight = 12,
                                    ProductID = 14,
                                    ProductName = "Aniseed Syrup",
                                    UnitPrice = 19,
                                    Quantity = 9,
                                    Discount = 0,
                                    Amount = 171,
                                    DiscountAmt = 0,
                                    NetAmount = 171
                                }
                            }
            };
        }

        private object Get16700431661089792()
        {
            return new
            {
                Table = new[] {
                       new
                        {
                            CustomerID = "HUNGC",
                            CompanyName = "五金机械",
                            ContactName = "苏先生",
                            ContactTitle = "销售代表",
                            Address = "德昌路甲 29 号",
                            City = "大连",
                            Region = "东北",
                            PostalCode = "564576",
                            Country = "中国",
                            Phone = "(053) 5556874",
                            Fax = "(053) 5552376"
                        },
                        new
                        {
                            CustomerID = "CENTC",
                            CompanyName = "三捷实业",
                            ContactName = "王先生",
                            ContactTitle = "市场经理",
                            Address = "英雄山路 84 号",
                            City = "大连",
                            Region = "东北",
                            PostalCode = "130083",
                            Country = "中国",
                            Phone = "(061) 15553392",
                            Fax = "(061) 15557293"
                        },
                        new
                        {
                            CustomerID = "CACTU",
                            CompanyName = "威航货运有限公司",
                            ContactName = "刘先生",
                            ContactTitle = "销售代理",
                            Address = "经七纬二路 13 号",
                            City = "大连",
                            Region = "东北",
                            PostalCode = "120412",
                            Country = "中国",
                            Phone = "(061) 11355555",
                            Fax = "(061) 11354892"
                        },
                        new
                        {
                            CustomerID = "BLONP",
                            CompanyName = "国皓",
                            ContactName = "黄雅玲",
                            ContactTitle = "市场经理",
                            Address = "广发北路 10 号",
                            City = "大连",
                            Region = "东北",
                            PostalCode = "565479",
                            Country = "中国",
                            Phone = "(0671) 88601531",
                            Fax = "(0671) 88601532"
                        },
                        new
                        {
                            CustomerID = "MEREP",
                            CompanyName = "华科",
                            ContactName = "吴小姐",
                            ContactTitle = "市场助理",
                            Address = "和光北路 952 号",
                            City = "大连",
                            Region = "东北",
                            PostalCode = "280235",
                            Country = "中国",
                            Phone = "(0514) 5558054",
                            Fax = "(0514) 5558055"
                        },
                        new
                        {
                            CustomerID = "CENTC",
                            CompanyName = "三捷实业",
                            ContactName = "王先生",
                            ContactTitle = "市场经理",
                            Address = "英雄山路 84 号",
                            City = "大连",
                            Region = "东北",
                            PostalCode = "130083",
                            Country = "中国",
                            Phone = "(061) 15553392",
                            Fax = "(061) 15557293"
                        },
                        new
                        {
                            CustomerID = "CACTU",
                            CompanyName = "威航货运有限公司",
                            ContactName = "刘先生",
                            ContactTitle = "销售代理",
                            Address = "经七纬二路 13 号",
                            City = "大连",
                            Region = "东北",
                            PostalCode = "120412",
                            Country = "中国",
                            Phone = "(061) 11355555",
                            Fax = "(061) 11354892"
                        },
                        new
                        {
                            CustomerID = "BLONP",
                            CompanyName = "国皓",
                            ContactName = "黄雅玲",
                            ContactTitle = "市场经理",
                            Address = "广发北路 10 号",
                            City = "大连",
                            Region = "东北",
                            PostalCode = "565479",
                            Country = "中国",
                            Phone = "(0671) 88601531",
                            Fax = "(0671) 88601532"
                        },
                        new
                        {
                            CustomerID = "MEREP",
                            CompanyName = "华科",
                            ContactName = "吴小姐",
                            ContactTitle = "市场助理",
                            Address = "和光北路 952 号",
                            City = "大连",
                            Region = "东北",
                            PostalCode = "280235",
                            Country = "中国",
                            Phone = "(0514) 5558054",
                            Fax = "(0514) 5558055"
                        },
                        new
                        {
                            CustomerID = "CENTC",
                            CompanyName = "三捷实业",
                            ContactName = "王先生",
                            ContactTitle = "市场经理",
                            Address = "英雄山路 84 号",
                            City = "大连",
                            Region = "东北",
                            PostalCode = "130083",
                            Country = "中国",
                            Phone = "(061) 15553392",
                            Fax = "(061) 15557293"
                        },
                        new
                        {
                            CustomerID = "CACTU",
                            CompanyName = "威航货运有限公司",
                            ContactName = "刘先生",
                            ContactTitle = "销售代理",
                            Address = "经七纬二路 13 号",
                            City = "大连",
                            Region = "东北",
                            PostalCode = "120412",
                            Country = "中国",
                            Phone = "(061) 11355555",
                            Fax = "(061) 11354892"
                        },
                        new
                        {
                            CustomerID = "BLONP",
                            CompanyName = "国皓",
                            ContactName = "黄雅玲",
                            ContactTitle = "市场经理",
                            Address = "广发北路 10 号",
                            City = "大连",
                            Region = "东北",
                            PostalCode = "565479",
                            Country = "中国",
                            Phone = "(0671) 88601531",
                            Fax = "(0671) 88601532"
                        },
                        new
                        {
                            CustomerID = "MEREP",
                            CompanyName = "华科",
                            ContactName = "吴小姐",
                            ContactTitle = "市场助理",
                            Address = "和光北路 952 号",
                            City = "大连",
                            Region = "东北",
                            PostalCode = "280235",
                            Country = "中国",
                            Phone = "(0514) 5558054",
                            Fax = "(0514) 5558055"
                        }
                }
            };
        }
    }
}