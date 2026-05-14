using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.DBManager;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.Extensions;
using VolPro.Core.Infrastructure;
using VolPro.Core.ManageUser;
using VolPro.Core.WorkFlow;
using VolPro.Entity.DomainModels;

namespace VolPro.Core.Print
{
    public class PrintContainer
    {
        public static List<PrintOptions> _print = new List<PrintOptions>();

        private static PrintContainer _instance;
        public static PrintContainer Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;

                }
                _instance = new PrintContainer();
                return _instance;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">打印分类的名称</param>
        /// <param name="printFields">可以打印的字段</param>
        /// <param name=""></param>
        /// <returns></returns>
        public PrintContainer Use<T>(string name, [NotNull] Expression<Func<T, object>> printFields, List<CustomField> customFields = null)
        {
            if (_print.Any(x => x.TableType == typeof(T)))
            {
                return Instance;
            }
            Type type = typeof(T);
            _print.Add(new PrintOptions()
            {
                TableType = type,
                Name = name ?? type.GetEntityTableCnName(),
                TableName = type.Name,
                Fields = printFields.GetExpressionProperty(),
                CustomFields = customFields
            });
            return Instance;
        }

        public PrintContainer Use<T, Detail>(
          string name,
          [NotNull] Expression<Func<T, object>> printFields,
          List<CustomField> customFields,
          string detailName,
          [NotNull] Expression<Func<Detail, object>> detailPrintFields,
          List<CustomField> detailCustomFields = null) where Detail : class
        {
            return Use(name, printFields, customFields, detail: new PrintDetailOptions<Detail>()
            {
                Name = detailName,
                PrintFields = detailPrintFields,
                CustomFields = detailCustomFields
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">打印的主表</typeparam>
        /// <typeparam name="Detail">打印的明细表</typeparam>
        /// <param name="name">打印分类的名称</param>
        /// <param name="printFields">可以打印的字段</param>
        ///  <param name="detailName">打印的明细表名称</param>
        /// <param name="printFields">可以打印的明细表字段</param>
        /// <param name=""></param>
        /// <returns></returns>
        public PrintContainer Use<T, Detail>(string name,
            [NotNull] Expression<Func<T, object>> printFields,
            string detailName,
            [NotNull] Expression<Func<Detail, object>> detailPrintFields
            ) where Detail : class
        {
            return Instance.Use(name, printFields, null, detailName, detailPrintFields);
        }
        public PrintContainer Use<T, Detail>(string name,
         [NotNull] Expression<Func<T, object>> printFields,
         List<CustomField> customFields,
        [NotNull] PrintDetailOptions<Detail> detail
         ) where Detail : class
        {
            return Instance.Use<T, Detail, Detail>(name, printFields, customFields, detail, null);
        }

        public PrintContainer Use<T, Detail1, Detail2>(string name,
        [NotNull] Expression<Func<T, object>> printFields,
        List<CustomField> customFields,
        [NotNull] PrintDetailOptions<Detail1> detail1,
        PrintDetailOptions<Detail2> detail2
        ) where Detail1 : class where Detail2 : class
        {
            return Instance.Use<T, Detail1, Detail2, Detail2>(name, printFields, customFields, detail1, detail2, null);
        }


        public PrintContainer Use<T, Detail1, Detail2, Detail3>(string name,
        [NotNull] Expression<Func<T, object>> printFields,
        List<CustomField> customFields,
        [NotNull] PrintDetailOptions<Detail1> detail1,
        PrintDetailOptions<Detail2> detail2,
        PrintDetailOptions<Detail3> detail3
        ) where Detail1 : class where Detail2 : class where Detail3 : class
        {
            if (_print.Any(x => x.TableType == typeof(T)))
            {
                return Instance;
            }
            Type type = typeof(T);


            var PrintDetails = new List<PrintDetail>();
            if (detail1 != null)
            {
                PrintDetails.Add(GetPrintDetail(detail1));
            }
            if (detail2 != null)
            {
                PrintDetails.Add(GetPrintDetail(detail2));
            }
            if (detail3 != null)
            {
                PrintDetails.Add(GetPrintDetail(detail3));
            }
            _print.Add(new PrintOptions()
            {
                TableType = type,
                Name = name ?? type.GetEntityTableCnName(),
                TableName = type.Name,
                Fields = printFields.GetExpressionProperty(),
                CustomFields = customFields,
                PrintDetails = PrintDetails
            });
            return Instance;
        }

        private PrintDetail GetPrintDetail<T>(PrintDetailOptions<T> detail) where T : class
        {
            return new PrintDetail()
            {
                DetailTableType = typeof(T),
                DetailName = detail.Name ?? typeof(T).GetEntityTableCnName(),
                DetailTableName = typeof(T).Name,
                DetailFields = detail.PrintFields.GetExpressionProperty(),
                CustomgFields = detail.CustomFields
            };
        }


        /// <summary>
        /// 获取下拉框数据源
        /// </summary>
        /// <returns></returns>
        public static object GetSelect()
        {
            //只显示有权限的表
            var permissions = UserContext.Current.Permissions;
            return _print
                .Where(x => permissions.Any(c => c.TableName == x.TableName.ToLower()))
                .Select(s => new { key = s.TableName, value = s.Name }).ToList();
        }

        /// <summary>
        /// 获取下拉框数据源
        /// </summary>
        /// <returns></returns>
        public static object GetOptions(string table)
        {
            //只显示有权限的表
            var permissions = UserContext.Current.Permissions;
            var query = _print
                   .Where(x => permissions.Any(c => c.TableName == x.TableName.ToLower()));
            if (!string.IsNullOrEmpty(table))
            {
                query = query.Where(x => x.TableName == table);
            }

            // .GroupBy(c=>new { c.TableName})
            var data = query.Select(s => new
            {
                s.TableName,
                s.Name,
                Fields = s.Fields.Select(x => x).ToList(),//.Union((s.CustomFields?.Select(s=>s.Field)??new List<string>()).ToList()).ToList(),
                details = (s.PrintDetails ?? new List<PrintDetail>())
                //.Select(c=>c.DetailFields)
                //s.DetailName,
                //s.DetailTableName,
                //DetailFields = (s.DetailFields ?? new string[] { }).Select(x => x).ToList().Union((s.DetailCustomgFields?.Select(s => s.Field) ?? new List<string>()).ToList()).ToList(),
            }).ToList();


            var tables = data.Select(s => s.TableName).ToList();

            //明细表配置
            var detailTableNames = data.SelectMany(x => x.details.Select(c => c.DetailTableName)).Where(x => x != null);
            tables.AddRange(detailTableNames);

            var tableOptions = DBServerProvider.DbContext.Set<Sys_TableColumn>().Where(x => tables.Contains(x.TableName))
                 .OrderByDescending(x => x.OrderNo)
                .Select(s => new { s.TableName, name = s.ColumnCnName, width = s.ColumnWidth, field = s.ColumnName, s.EditType, s.DropNo, TableType = s.IsImage })
                .ToList();

            var options = data.Select(s => new
            {
                s.TableName,
                s.Name,
                Fields = tableOptions.Where(c => c.TableName == s.TableName && s.Fields.Contains(c.field)).ToList(),
                //明细表
                TemplateDetails = s.details.Select(c => new
                {
                    c.DetailName,
                    c.DetailTableName,
                    DetailFields = tableOptions.Where(x => x.TableName == c.DetailTableName && c.DetailFields.Contains(x.field))
                                   .Select(s => new { s.field, s.name, TableType = s.TableType ?? -1 }).ToList()
                                   .Union((c.CustomgFields ?? new List<CustomField>()).Select(s => new { field = s.Field, name = s.Name, TableType = -1 }))
                })
            }).ToList();
            return options;
        }

        public static async Task<object> GetPrintDataAsync(PrintQuery query)
        {

            var ops = _print.Where(x => x.TableName == query.Table).FirstOrDefault();
            //if ((query.Detail || query.BatchMainAndDetail) && ops.DetailTableType == null)
            //{
            //    throw new Exception($"startup里面必须配置[{ops.TableName}]的明细表");
            //}

            query.TemplateName = await DBServerProvider.DbContext.Set<Sys_PrintOptions>()
                .Where(x => x.PrintOptionsId == query.TemplateId)
                .Select(ops => ops.CustomName).FirstOrDefaultAsync();

            var list = typeof(PrintContainer).GetMethod("GetDataAsync")
            .MakeGenericMethod(new Type[] { ops.TableType })
            .Invoke(null, new object[] { query.Ids, query.Table, ops.Fields.Select(x => x).ToList(), ops.PrintDetails, query, ops }) as Task<object>;
            return await list;
        }

        /// <summary>
        /// 获取明细表配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids"></param>
        /// <param name="table"></param>
        /// <param name="fields"></param>
        /// <param name="printDetails"></param>
        /// <param name="parms"></param>
        /// <param name="ops"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<object> GetDataAsync<T>(object[] ids, string table, List<string> fields, List<PrintDetail> printDetails, PrintQuery parms, PrintOptions ops)
            where T : class //where Detail : class
        {
            if (ids == null || ids.Length == 0)
            {
                return new List<Dictionary<string, object>>();
            }
            string key = typeof(T).GetKeyName();

            if (parms.Detail || (printDetails?.Count > 0))
            {
                fields.Add(key);
            }
            //else if (parms.BatchMainAndDetail || parms.BatchMain)
            //{

            //}
            //else
            //{
            //    //不是批量打印的，默认只查第一条
            //    ids = new object[] { ids[0] };
            //}
            PrintFilter filter = new PrintCustom();
            var express = key.CreateExpression<T>(ids, Enums.LinqExpressionType.In);
            var dbContext = DBServerProvider.GetEntityDbContext<T>();
            var queryable = dbContext.Set<T>().Where(express);

            queryable = filter.Query(queryable, parms);

            var entities = await queryable.ToListAsync();
            //主表数据
            List<Dictionary<string, object>> list = await ConvertListAsync(entities, ids, table, fields, parms);

            if (printDetails == null)
            {
                printDetails = new List<PrintDetail>();
            }
            foreach (var mainData in list)
            {
                //获取明细表
                foreach (var detail in printDetails)
                {
                    var pro = detail.DetailTableType.GetProperty(key);
                    if (pro == null)
                    {
                        throw new Exception($"明细表必须包括主表字段[{key}]");
                    }
                    //var detailExpress = key.CreateExpression<Detail>(ids, Enums.LinqExpressionType.In);
                    //var detailQueryable = DBServerProvider.GetEFDbContext<Detail>().Set<Detail>().Where(detailExpress);
                    //detailQueryable = filter.QueryDetail(detailQueryable, parms);
                    //var detailEntities = await detailQueryable.ToListAsync();
                    fields = detail.DetailFields.ToList();
                    fields.Add(key);
                    //var detailList = await ConvertListAsync(detailEntities, ids, detail.DetailTableName, fields, parms);

                    var obj = typeof(PrintContainer).GetMethod("GetDetailDataAsync")
                                      .MakeGenericMethod(new Type[] { detail.DetailTableType })
                                      .Invoke(null, new object[] { key, ids, fields, parms });
                    var detailList = await (obj as Task<List<Dictionary<string, object>>>);

                    string keyValue = mainData[key].ToString();
                    List<Dictionary<string, object>> detailDic = new List<Dictionary<string, object>>(); ;
                    foreach (var detailItem in detailList)
                    {
                        if (detailItem[key].ToString() == keyValue)
                        {
                            detailDic.Add(detailItem);
                        }
                    }
                    mainData[detail.DetailTableName] = detailDic;
                }
            }
            list = filter.QueryResult<T>(list, parms, dbContext);
            return list;
        }

        public static async Task<List<Dictionary<string, object>>> GetDetailDataAsync<Detail>(string key, object[] ids, List<string> fields, PrintQuery parms)
            where Detail : class
        {
            PrintFilter filter = new PrintCustom();
            var detailExpress = key.CreateExpression<Detail>(ids, Enums.LinqExpressionType.In);
            var detailQueryable = DBServerProvider.GetEntityDbContext<Detail>().Set<Detail>().Where(detailExpress);
            detailQueryable = filter.QueryDetail(detailQueryable, parms);
            var detailEntities = await detailQueryable.ToListAsync();
            var data = await ConvertListAsync(detailEntities, ids, typeof(Detail).Name, fields, parms);

            return data;
        }


        private static async Task<List<Dictionary<string, object>>> ConvertListAsync<T>(List<T> entities, object[] ids, string table, List<string> fields, PrintQuery query)
        {
            var columns = await DBServerProvider.DbContext.Set<Sys_TableColumn>().Where(c => c.TableName == table).ToListAsync();
            var tableOptions = columns.Where(c => fields.Contains(c.ColumnName)).
                 Select(s => new { s.ColumnName, s.ColumnCnName, s.DropNo, isDate = s.IsImage == 4 || s.EditType == "date" });

            List<Sys_Dictionary> dictionaries = new List<Sys_Dictionary>();
            var dicNos = tableOptions.Select(s => s.DropNo).ToList();
            if (dicNos.Count > 0)
            {
                dictionaries = DictionaryManager.GetDictionaries(dicNos, true).ToList();
            }

            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            var properties = typeof(T).GetProperties();

            foreach (var data in entities)
            {
                Dictionary<string, object> item = new Dictionary<string, object>();
                foreach (var field in fields)
                {
                    var property = properties.Where(c => c.Name == field).FirstOrDefault();
                    string value = property.GetValue(data)?.ToString();

                    var option = tableOptions.Where(c => c.ColumnName == field).FirstOrDefault();
                    string name = option?.ColumnCnName;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = property.GetDisplayName();
                    }
                    if (option == null || string.IsNullOrEmpty(value))
                    {
                        item[property.Name] = value = value ?? "";
                        continue;
                    }
                    if (option.isDate)
                    {
                        value = value.GetDateTime().Value.ToString("yyyy-MM-dd");
                    }
                    else if (!string.IsNullOrEmpty(option.DropNo))
                    {
                        string val = dictionaries.Where(c => c.DicNo == option.DropNo).FirstOrDefault()
                             ?.Sys_DictionaryList
                             //这里多选的暂时没处理
                             ?.Where(c => c.DicValue == value)?.Select(s => s.DicName)
                             .FirstOrDefault();
                        if (!string.IsNullOrEmpty(val))
                        {
                            value = val;
                        }
                    }
                    item[property.Name] = value;
                }
                list.Add(item);
            }
            return list;
        }


    }
}
