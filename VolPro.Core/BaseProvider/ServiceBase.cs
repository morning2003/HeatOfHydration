using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VolPro.Core.Configuration;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.Enums;
using VolPro.Core.Extensions;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Core.Filters;
using VolPro.Core.ManageUser;
using VolPro.Core.Services;
using VolPro.Core.Tenancy;
using VolPro.Core.UserManager;
using VolPro.Core.Utilities;
using VolPro.Core.WorkFlow;
using VolPro.Entity;
using VolPro.Entity.DomainModels;
using VolPro.Entity.SystemModels;
using CC = VolPro.Core.CacheManager;

namespace VolPro.Core.BaseProvider
{
    public abstract class ServiceBase<T, TRepository> : ServiceFunFilter<T>
            where T : BaseEntity, new()
            where TRepository : IRepository<T>
    {
        public ServiceBase()
        {

        }
        public CC.ICacheService CacheContext
        {
            get
            {
                return AutofacContainerModule.GetService<CC.ICacheService>();
            }
        }

        public Microsoft.AspNetCore.Http.HttpContext Context
        {
            get
            {
                return HttpContext.Current;
            }
        }
        private WebResponseContent Response { get; set; }

        protected IRepository<T> repository;

        private PropertyInfo[] _propertyInfo { get; set; } = null;
        private PropertyInfo[] TProperties
        {
            get
            {
                if (_propertyInfo != null)
                {
                    return _propertyInfo;
                }
                _propertyInfo = typeof(T).GetProperties();
                return _propertyInfo;
            }
        }

        public ServiceBase(TRepository repository)
        {
            Response = new WebResponseContent(true);
            this.repository = repository;
        }

        protected virtual void Init(IRepository<T> repository)
        {

        }

        protected virtual Type GetRealDetailType()
        {
            return typeof(T).GetCustomAttribute<EntityAttribute>()?.DetailTable?[0];
        }

        /// <summary>
        ///  2020.08.15添加自定义原生查询sql或多租户(查询、导出)
        /// </summary>
        /// <returns></returns>
        private ISugarQueryable<T> GetSearchQueryable()
        {
            if (QuerySql != null)
            {
                return repository.DbContext.SqlQueryable<T>(QuerySql.ToString());
            }
            string tableName = typeof(T).GetEntityTableName();

            var query = repository.DbContext.Queryable<T>().FilterTenancy<T>();
            var result = TenancyManager<T>.GetSearchQueryable(QuerySql, tableName, query);
            if (!string.IsNullOrEmpty(result.sql))
            {
                return repository.DbContext.SqlQueryable<T>(result.sql);
            }
            return result.query;
        }

        /// <summary>
        ///  2020.08.15添加获取多租户数据过滤sql（删除、编辑）
        /// </summary>
        /// <returns></returns>
        private string GetMultiTenancySql(string ids, string tableKey)
        {
            return TenancyManager<T>.GetMultiTenancySql(typeof(T).GetEntityTableName(), ids, tableKey);
        }

        /// <summary>
        ///  2020.08.15添加多租户数据过滤（删除）
        /// </summary>
        private void CheckDelMultiTenancy(string ids, string tableKey)
        {
            //string sql = GetMultiTenancySql(ids, tableKey);
            //if (string.IsNullOrEmpty(sql))
            //{
            //    return;
            //}

            ////请接着过滤条件
            ////例如sql，只能(删除)自己创建的数据:找出不是自己创建的数据
            ////sql = $" {sql} and CreateId!={UserContext.Current.UserId}";
            //object obj = repository.DapperContext.ExecuteScalar(sql, null);
            //int idsCount = ids.Split(",").Distinct().Count();
            //if (obj == null || obj.GetInt() != idsCount)
            //{
            //    Response.Error("不能删除此数据");
            //}
        }

        private const string _asc = "asc";
        /// <summary>
        /// 生成排序字段
        /// </summary>
        /// <param name="pageData"></param>
        /// <param name="propertyInfo"></param>
        private Dictionary<string, QueryOrderBy> GetPageDataSort(PageDataOptions pageData, PropertyInfo[] propertyInfo)
        {
            if (base.OrderByExpression != null)
            {
                return base.OrderByExpression.GetExpressionToDic();
            }
            if (!string.IsNullOrEmpty(pageData.Sort))
            {
                if (pageData.Sort.Contains(","))
                {
                    var sortArr = pageData.Sort.Split(",").Where(x => propertyInfo.Any(c => c.Name == x)).Select(s => s).Distinct().ToList();
                    Dictionary<string, QueryOrderBy> sortDic = new Dictionary<string, QueryOrderBy>();
                    foreach (var name in sortArr)
                    {
                        sortDic[name] = pageData.Order?.ToLower() == _asc ? QueryOrderBy.Asc : QueryOrderBy.Desc;
                    }
                    return sortDic;
                }
                else if (propertyInfo.Any(x => x.Name == pageData.Sort))
                {
                    return new Dictionary<string, QueryOrderBy>() { {
                            pageData.Sort,
                            pageData.Order?.ToLower() == _asc? QueryOrderBy.Asc: QueryOrderBy.Desc
                     } };
                }
            }
            //如果没有排序字段，则使用主键作为排序字段

            PropertyInfo property = propertyInfo.GetKeyProperty();
            //如果主键不是自增类型则使用appsettings.json中CreateMember->DateField配置的创建时间作为排序
            if (property.PropertyType == typeof(int) || property.PropertyType == typeof(long))
            {
                if (!propertyInfo.Any(x => x.Name.ToLower() == pageData.Sort))
                {
                    pageData.Sort = propertyInfo.GetKeyName();
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(AppSetting.CreateMember.DateField)
                    && propertyInfo.Any(x => x.Name == AppSetting.CreateMember.DateField))
                {
                    pageData.Sort = AppSetting.CreateMember.DateField;
                }
                else
                {
                    pageData.Sort = propertyInfo.GetKeyName();
                }
            }
            return new Dictionary<string, QueryOrderBy>() { {
                    pageData.Sort, pageData.Order?.ToLower() == _asc? QueryOrderBy.Asc: QueryOrderBy.Desc
                } };
        }

        /// <summary>
        /// 前端查询条件转换为EF查询Queryable(2023.04.02)
        /// </summary>
        /// <param name="options">前端查询参数</param>
        /// <param name="useTenancy">是否使用数据隔离</param>
        /// <returns></returns>
        public ISugarQueryable<T> GetPageDataQueryFilter(PageDataOptions options, bool useTenancy = true)
        {
            ValidatePageOptions(options, out ISugarQueryable<T> queryable, useTenancy);
            return queryable;
        }

        /// <summary>
        /// 验证排序与查询字段合法性
        /// </summary>
        /// <param name="options"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        protected PageDataOptions ValidatePageOptions(PageDataOptions options, out ISugarQueryable<T> queryable, bool useTenancy = true)
        {
            options = options ?? new PageDataOptions();

            List<SearchParameters> searchParametersList = new List<SearchParameters>();
            if (options.Filter != null && options.Filter.Count > 0)
            {
                searchParametersList.AddRange(options.Filter);
            }
            else if (!string.IsNullOrEmpty(options.Wheres))
            {
                try
                {
                    searchParametersList = options.Wheres.DeserializeObject<List<SearchParameters>>();
                    options.Filter = searchParametersList;
                }
                catch { }
            }
            QueryRelativeList?.Invoke(searchParametersList);
            if (useTenancy)
            {
                queryable = GetSearchQueryable();
            }
            else
            {
                queryable = repository.DbContext.Set<T>();
            }
            //  Connection
            // queryable = repository.DbContext.Set<T>();
            //2020.08.15添加自定义原生查询sql或多租户


            //判断列的数据类型数字，日期的需要判断值的格式是否正确
            for (int i = 0; i < searchParametersList.Count; i++)
            {
                SearchParameters x = searchParametersList[i];
                if (string.IsNullOrEmpty(x.Value))
                {
                    continue;
                }
                // x.DisplayType = x.DisplayType.GetDbCondition();
                PropertyInfo property = TProperties.Where(c => c.Name.ToUpper() == x.Name.ToUpper()).FirstOrDefault();
                //2020.06.25增加字段null处理
                if (property == null) continue;
                // property
                //移除查询的值与数据库类型不匹配的数据
                object[] values = property.ValidationValueForDbType(x.Value.Split(',')).Where(q => q.Item1).Select(s => s.Item3).ToArray();
                if (values == null || values.Length == 0)
                {
                    continue;
                }
                LinqExpressionType expressionType = x.DisplayType.GetLinqCondition();
                //if (x.DisplayType == HtmlElementType.Contains)
                //    x.Value = string.Join(",", values);
                queryable = LinqExpressionType.In == expressionType
                              ? queryable.Where(x.Name.CreateExpression<T>(values, expressionType))
                              : queryable.Where(x.Name.CreateExpression<T>(x.Value, expressionType));
            }
            options.TableName = base.TableName ?? typeof(T).Name;
            return options;
        }

        /// <summary>
        /// 加载页面数据
        /// </summary>
        /// <param name="loadSingleParameters"></param>
        /// <returns></returns>
        public virtual PageGridData<T> GetPageData(PageDataOptions options)
        {
            options = ValidatePageOptions(options, out ISugarQueryable<T> queryable, IsMultiTenancy);
            //获取排序字段
            Dictionary<string, QueryOrderBy> orderbyDic = GetPageDataSort(options, TProperties);

            PageGridData<T> pageGridData = new PageGridData<T>();
            if (QueryRelativeExpression != null)
            {
                queryable = QueryRelativeExpression.Invoke(queryable);
            }

            //过滤逻辑删除
            var logicDelProperty = GetLogicDelProperty<T>();
            if (logicDelProperty != null)
            {
                queryable = queryable.Where(logicDelProperty.Name.CreateExpression<T>(((int)DelStatus.正常).ChangeType(logicDelProperty.PropertyType), LinqExpressionType.Equal));
            }


            if (options.Export)
            {
                queryable = queryable.GetISugarQueryableOrderBy(orderbyDic);
                if (Limit > 0)
                {
                    queryable = queryable.Take(Limit);
                }
                pageGridData.rows = FilterQueryableAuthFields(queryable);
            }
            else
            {
                queryable = repository.IQueryablePage(queryable,
                               options.Page,
                               options.Rows,
                               out int rowCount,
                               orderbyDic);
                pageGridData.rows = FilterQueryableAuthFields(queryable);
                pageGridData.total = rowCount;

                //查询界面统计求等字段
                //这里sqlsugar会把IQueryablePage里面的排序字段也添加进去了就会导致异常2023.10.17
                if (SummaryExpress != null)
                {
                    options = ValidatePageOptions(options, out ISugarQueryable<T> queryableSummary);
                    //if (QueryRelativeExpression!=null)
                    //{
                    //    queryableSummary = QueryRelativeExpression.Invoke(queryableSummary);
                    //}
                    pageGridData.summary = SummaryExpress.Invoke(queryableSummary);
                }

            }
            GetPageDataOnExecuted?.Invoke(pageGridData);
            return pageGridData;

        }
        /// <summary>
        /// 映射指定权限的字段不查询数据库2023.08.03
        /// </summary>
        /// <param name="queryable"></param>
        /// <returns></returns>
        private List<T> FilterQueryableAuthFields(ISugarQueryable<T> queryable)
        {
            string tableName = typeof(T).Name;
            var authFields = RoleContext.GetCurrentRoleAuthFields(tableName);
            if (authFields.Length == 0)
            {
                return queryable.ToList();
            }
            var source = typeof(T);
            var target = typeof(T);

            var t = Expression.Parameter(source, "t");

            List<MemberAssignment> assignments = new();

            //获取代码生成器隐藏的字段
            var hideFields = TableColumnContext.GetTableHideFields(tableName);

            var fields = source.GetProperties().Where(x => authFields.Contains(x.Name) || hideFields.Contains(x.Name)).Select(s => s.Name).ToList();

            foreach (var item in fields)
            {
                var member1 = Expression.MakeMemberAccess(t, source.GetProperty(item));

                var member2 = Expression.Bind(target.GetProperty(item), member1);
                assignments.Add(member2);
            }
            var newExpression = Expression.New(target);
            var memberInit = Expression.MemberInit(newExpression, assignments);
            var expression = (Expression<Func<T, T>>)Expression.Lambda(memberInit, t);
            return queryable.Select(expression).ToList();

        }
        public virtual object GetDetailPage(PageDataOptions pageData)
        {
            var tables = typeof(T).GetCustomAttribute<EntityAttribute>();
            if (tables == null)
            {
                return null;
            }
            string keyName = typeof(T).GetKeyName();

            Type detailType = null;

            if (string.IsNullOrEmpty(pageData.TableName) && string.IsNullOrEmpty(pageData.DetailTable))
            {
                detailType = tables.DetailTable.FirstOrDefault();
            }
            else
            {  //三级明细表查询
                if (!string.IsNullOrEmpty(pageData.DetailTable))
                {
                    //获取二级明细表
                    detailType = tables.DetailTable.Where(c => c.Name == pageData.DetailTable).FirstOrDefault();
                    keyName = detailType.GetKeyName();
                    detailType = detailType.GetCustomAttribute<EntityAttribute>()?.DetailTable
                                       ?.Where(x => x.Name == pageData.TableName)?.FirstOrDefault();
                }
                else
                {
                    //多表二级明细表查询
                    detailType = tables.DetailTable.Where(c => c.Name == pageData.TableName).FirstOrDefault();
                }
            }

            if (detailType == null)
            {
                string message = $"未找到配置{pageData.TableName},请检查代码生成器明细表配置及是否生成model";
                Console.WriteLine(message);
                return new { message = message };
            }
            object obj = typeof(ServiceBase<T, TRepository>)
                 .GetMethod("GetDetailPage", BindingFlags.Instance | BindingFlags.NonPublic)
                 .MakeGenericMethod(new Type[] { detailType }).Invoke(this, new object[] { pageData, keyName });
            return obj;
        }
        protected override object GetDetailSummary<Detail>(ISugarQueryable<Detail> queryeable)
        {
            return null;
        }

        private PageGridData<Detail> GetDetailPage<Detail>(PageDataOptions options, string keyName) where Detail : class
        {
            //校验查询值，排序字段，分页大小规则待完
            PageGridData<Detail> gridData = new PageGridData<Detail>();
            if (options.Value == null) return gridData;
            ////主表主键字段
            //string keyName = typeof(T).GetKeyName();

            //生成查询条件
            Expression<Func<Detail, bool>> whereExpression = keyName.CreateExpression<Detail>(options.Value, LinqExpressionType.Equal);

            var queryeable = repository.DbContext.Set<Detail>().Where(whereExpression);

            gridData.total = queryeable.Count();
            options.Sort = options.Sort ?? typeof(Detail).GetKeyName();
            Dictionary<string, QueryOrderBy> orderBy = GetPageDataSort(options, typeof(Detail).GetProperties());

            gridData.rows = queryeable
                 .GetISugarQueryableOrderBy(orderBy)
                .Skip((options.Page - 1) * options.Rows)
                .Take(options.Rows)
                .ToList();

            //查询界面统计求等字段
            //这里sqlsugar会把IQueryablePage里面的排序字段也添加进去了就会导致异常2023.10.17
            queryeable = repository.DbContext.Set<Detail>().Where(whereExpression);
            gridData.summary = GetDetailSummary<Detail>(queryeable);
            return gridData;
        }



        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public virtual WebResponseContent Upload(List<Microsoft.AspNetCore.Http.IFormFile> files)
        {
            if (files == null || files.Count == 0) return Response.Error("请上传文件");

            string filePath;
            if (!string.IsNullOrEmpty(UploadFolder))
            {
                filePath = UploadFolder;
                if (!filePath.EndsWith("/") || !filePath.EndsWith("\\"))
                {
                    filePath += "/";
                }
            }
            else
            {
                filePath = $"Upload/Tables/{typeof(T).GetEntityTableName()}/{DateTime.Now.ToString("yyyMMddHHmmsss") + new Random().Next(1000, 9999)}/";
            }

            string fullPath = filePath.MapPath(IsRoot);
            int i = 0;
            try
            {
                if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
                for (i = 0; i < files.Count; i++)
                {
                    string fileName = HttpContext.Current.Request("fileName");
                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = files[i].FileName;
                    }
                    using var stream = new FileStream(fullPath + fileName, FileMode.Create);
                    files[i].CopyTo(stream);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"上传文件失败：{typeof(T).GetEntityTableCnName()},路径：{filePath},失败文件:{files[i]},{ex.Message + ex.StackTrace}");
                return Response.Error("文件上传失败".Translator());
            }
            return Response.OK("文件上传成功".Translator(), filePath);
        }

        private List<string> GetIgnoreTemplate()
        {
            //忽略创建人、修改人、审批等字段
            List<string> ignoreTemplate = UserIgnoreFields.ToList();
            ignoreTemplate.AddRange(auditFields.ToList());
            return ignoreTemplate;
        }

        public virtual WebResponseContent DownLoadTemplate()
        {
            string tableName = typeof(T).GetEntityTableCnName();

            string dicPath = $"Download/{DateTime.Now.ToString("yyyMMdd")}/Template/".MapPath();
            if (!Directory.Exists(dicPath)) Directory.CreateDirectory(dicPath);
            string fileName = tableName + DateTime.Now.ToString("yyyyMMddHHssmm") + ".xlsx";
            //DownLoadTemplateColumns 2020.05.07增加扩展指定导出模板的列
            EPPlusHelper.ExportTemplate<T>(DownLoadTemplateColumns, GetIgnoreTemplate(), dicPath, fileName, ExcelHeaderMap);
            return Response.OK(null, dicPath + fileName);
        }

        /// <summary>
        /// 导入表数据Excel文件夹
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public virtual WebResponseContent Import(List<Microsoft.AspNetCore.Http.IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return new WebResponseContent { Status = true, Message = "请选择上传的文件".Translator() };
            Microsoft.AspNetCore.Http.IFormFile formFile = files[0];
            string dicPath = $"Upload/{DateTime.Now.ToString("yyyMMdd")}/{typeof(T).Name}/".MapPath();
            if (!Directory.Exists(dicPath)) Directory.CreateDirectory(dicPath);
            dicPath = $"{dicPath}{Guid.NewGuid().ToString()}_{formFile.FileName}";

            using (var stream = new FileStream(dicPath, FileMode.Create))
            {
                formFile.CopyTo(stream);
            }
            try
            {
                //2022.06.20增加原生excel读取方法(导入时可以自定义读取excel内容)
                //2022.06.20增加原生excel读取方法(导入时可以自定义读取excel内容)
                Response = EPPlusHelper.ReadToDataTable<T>(dicPath, DownLoadTemplateColumns, GetIgnoreTemplate(), readValue: ImportOnReadCellValue,
                    ExcelHeaderMap, ImportStartRowIndex, ImportIgnoreSelectValidationColumns);
            }
            catch (Exception ex)
            {
                Response.Error("未能处理导入的文件,请检查导入的文件是否正确".Translator());
                Logger.Error($"表{typeof(T).GetEntityTableCnName()}导入失败{ex.Message + ex.InnerException?.Message}");
            }
            if (CheckResponseResult()) return Response;
            List<T> list = Response.Data as List<T>;

            var logicDelProperty = GetLogicDelProperty<T>();
            if (logicDelProperty != null)
            {
                foreach (var item in list)
                {
                    logicDelProperty.SetValue(item, ((int)DelStatus.正常).ChangeType(logicDelProperty.PropertyType));
                }
            }

            var keyPro = typeof(T).GetKeyProperty();
            if (keyPro.PropertyType == typeof(long) && AppSetting.UseSnow)
            {
                //生成雪花id
                var idWorker = new Utilities.IdWorker();
                foreach (var item in list)
                {
                    keyPro.SetValue(item, idWorker.NextId());
                }
            }
            else if (keyPro == typeof(Guid))
            {
                foreach (var item in list)
                {
                    keyPro.SetValue(item, Guid.NewGuid());
                }
            }
            list.SetTenancyValue().CreateCode();

            if (ImportOnExecuting != null)
            {
                Response = ImportOnExecuting.Invoke(list);
                if (CheckResponseResult()) return Response;
            }
            //2022.01.08增加明细表导入判断
            if (HttpContext.Current.Request.Query.ContainsKey("table"))
            {
                ImportOnExecuted?.Invoke(list);
                return Response.OK("文件上传成功".Translator(), list.Serialize());
            }
            repository.SqlSugarClient.Insertable<T>(list).ExecuteCommand();
            if (ImportOnExecuted != null)
            {
                Response = ImportOnExecuted.Invoke(list);
                if (CheckResponseResult()) return Response;
            }
            return Response.OK("文件上传成功".Translator());
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="pageData"></param>
        /// <returns></returns>
        public virtual WebResponseContent Export(PageDataOptions pageData)
        {
            pageData.Export = true;
            List<T> list = GetPageData(pageData).rows;
            string tableName = typeof(T).GetEntityTableCnName();
            string fileName = tableName + DateTime.Now.ToString("yyyyMMddHHssmm") + ".xlsx";
            string folder = DateTime.Now.ToString("yyyyMMdd");
            string savePath = $"Download/ExcelExport/{folder}/".MapPath();
            List<string> ignoreColumn = new List<string>();
            if (ExportOnExecuting != null)
            {
                Response = ExportOnExecuting(list, ignoreColumn);
                if (CheckResponseResult()) return Response;
            }

            var exportFields = ExportColumns?.GetExpressionToArray() ?? new string[] { };
            //2024.02.03增加导出列表与界面显示字段一致
            if (exportFields == null || exportFields.Length == 0)
            {
                if (pageData.Columns != null && pageData.Columns.Length > 0)
                {
                    exportFields = pageData.Columns;
                }
            }
            var fields = RoleContext.GetCurrentRoleAuthFields(typeof(T).Name);
            if (fields.Length > 0)
            {
                var _arr = exportFields.ToList();
                _arr.AddRange(fields);
                exportFields = _arr.Distinct().ToArray();
            }
            if (ignoreColumn.Count > 0)
            {
                ignoreColumn = ignoreColumn.Distinct().ToList();
            }
            //ExportColumns 2020.05.07增加扩展指定导出模板的列
            EPPlusHelper.Export(list, exportFields, ignoreColumn, savePath, fileName);
            //return Response.OK(null, (savePath + "/" + fileName).EncryptDES(AppSetting.Secret.ExportFile));
            //2022.01.08优化导出功能
            return Response.OK(null, (savePath + "/" + fileName));
        }

        /// <summary>
        /// 新建
        /// </summary>
        /// <param name="saveDataModel"></param>
        /// <returns></returns>
        public virtual WebResponseContent Add(SaveModel saveDataModel)
        {
            if (AddOnExecute != null)
            {
                Response = AddOnExecute(saveDataModel);
                if (CheckResponseResult()) return Response;
            }
            if (saveDataModel == null
                || saveDataModel.MainData == null
                || saveDataModel.MainData.Count == 0)
                return Response.Set(ResponseType.ParametersLack, false);

            saveDataModel.DetailData = saveDataModel.DetailData?.Where(x => x.Count > 0).ToList();
            Type type = typeof(T);
            //adams 2023.10.31
            PropertyInfo keyPro = type.GetKeyProperty();
            string validReslut = type.ValidateDicInEntity(saveDataModel.MainData, removeNotContains: true, removerKey: keyPro.PropertyType != typeof(string), UserIgnoreFields);

            if (!string.IsNullOrEmpty(validReslut)) return Response.Error(validReslut);

            if (saveDataModel.MainData.Count == 0)
                return Response.Error("保存的数据为空，请检查model是否配置正确!");

            //过滤逻辑删除
            var logicDelProperty = GetLogicDelProperty<T>();
            if (logicDelProperty != null)
            {
                saveDataModel.MainData[logicDelProperty.Name] = (int)DelStatus.正常;
            }
            //2024.06.10增加数据版本号管理
            if (!string.IsNullOrEmpty(saveDataModel.DataVersionField))
            {
                saveDataModel.MainData.TryAdd(saveDataModel.DataVersionField, Guid.NewGuid().ToString());
            }
            UserInfo userInfo = UserContext.Current.UserInfo;
            saveDataModel.SetDefaultVal(AppSetting.CreateMember, userInfo);

            //主键值
            object keyProVal = null;
            if (saveDataModel.MainData.ContainsKey(keyPro.Name))
                keyProVal = saveDataModel.MainData[keyPro.Name];

            if (keyPro.PropertyType == typeof(Guid))
            {
                saveDataModel.MainData.Add(keyPro.Name, Guid.NewGuid());
            }
            else if (keyPro.PropertyType == typeof(long) && AppSetting.UseSnow)
            {
                saveDataModel.MainData.Add(keyPro.Name, new Utilities.IdWorker().NextId());
            }
            else if (keyPro.PropertyType == typeof(string))
            {
                if (keyProVal.IsNullOrEmpty())
                    saveDataModel.MainData[keyPro.Name] = new Utilities.IdWorker().NextId().ToString();  //adams 2023.4.27 雪花算法
            }
            else
            {
                saveDataModel.MainData.Remove(keyPro.Name);
            }

            //一对多
            if (saveDataModel.Details != null && saveDataModel.Details.Count() > 0)
            {
                return AddMultipleDetail(saveDataModel);
            }

            //没有明细直接保存返回
            if (saveDataModel.DetailData == null || saveDataModel.DetailData.Count == 0)
            {
                T mainEntity = saveDataModel.MainData.DicToEntity<T>();
                SetAuditDefaultValue(mainEntity);
                mainEntity.SetTenancyValue().SetValue();
                mainEntity.CreateCode<T>();
                if (base.AddOnExecuting != null)
                {
                    Response = base.AddOnExecuting(mainEntity, null);
                    if (CheckResponseResult()) return Response;
                }
                Response = repository.DbContextBeginTransaction(() =>
                {
                    SetEntityValue(mainEntity);
                    repository.AddWithSetIdentity(mainEntity);
                    saveDataModel.MainData[keyPro.Name] = keyPro.GetValue(mainEntity);
                    Response.OK(ResponseType.SaveSuccess);
                    if (base.AddOnExecuted != null)
                    {
                        Response = base.AddOnExecuted(mainEntity, null);
                    }
                    return Response;
                });
                if (Response.Status) Response.Data = new { data = mainEntity };
                AddProcese(mainEntity);
                return Response;
            }

            Type detailType = GetRealDetailType();

            return typeof(ServiceBase<T, TRepository>)
                .GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(new Type[] { detailType })
                .Invoke(this, new object[] { saveDataModel })
                as WebResponseContent;
        }

        public virtual WebResponseContent AddEntity(T entity, bool validationEntity = true)
        {
            return Add<T>(entity, null, validationEntity);
        }



        /// <summary>
        /// 保存主、明细数据
        /// </summary>
        /// <typeparam name="TDetail"></typeparam>
        /// <param name="entity"></param>
        /// <param name="list"></param>
        /// <param name="validationEntity">是否进行实体验证</param>
        /// <returns></returns>
        public WebResponseContent Add<TDetail>(T entity, List<TDetail> list = null, bool validationEntity = true) where TDetail : class, new()
        {
            //设置用户默认值
            entity.SetCreateDefaultVal();
            SetAuditDefaultValue(entity);
            if (validationEntity)
            {
                Response = entity.ValidationEntity();
                if (CheckResponseResult()) return Response;
                if (list != null && list.Count > 0)
                {
                    Response = list.ValidationEntity();
                    if (CheckResponseResult()) return Response;
                }
            }
            var logicDelEntityProperty = GetLogicDelProperty<T>();
            if (logicDelEntityProperty != null)
            {
                logicDelEntityProperty.SetValue(entity, ((int)DelStatus.正常).ChangeType(logicDelEntityProperty.PropertyType));
            }
            entity.SetTenancyValue().SetValue();
            entity.CreateCode<T>();
            if (AddOnExecuting != null)
            {
                Response = AddOnExecuting(entity, list);
                if (CheckResponseResult()) return Response;
            }
            Response = repository.DbContextBeginTransaction(() =>
            {
                SetEntityValue(entity);
                repository.AddWithSetIdentity(entity);

                //repository.DbContext.SaveChanges();
                //保存明细
                if (list != null && list.Count > 0)
                {

                    //过滤逻辑删除
                    var logicDelProperty = GetLogicDelProperty<TDetail>();
                    //获取保存后的主键值
                    PropertyInfo mainKey = typeof(T).GetKeyProperty();
                    PropertyInfo detailMainKey = typeof(TDetail).GetProperties()
                        .Where(q => q.Name.ToLower() == mainKey.Name.ToLower()).FirstOrDefault();
                    object keyValue = mainKey.GetValue(entity);

                    //adams 2023.5.30
                    PropertyInfo detailKey = typeof(TDetail).GetKeyProperty();
                    var idWorker = new Utilities.IdWorker();
                    list.ForEach(x =>
                    {
                        //adams 2023.6.17
                        if (detailKey.PropertyType == typeof(string))
                        {
                            detailKey.SetValue(x, idWorker.NextId().ToString());
                        }
                        if (logicDelProperty != null)
                        {
                            logicDelProperty.SetValue(x, ((int)DelStatus.正常).ChangeType(logicDelProperty.PropertyType));
                        }
                        //设置用户默认值
                        x.SetCreateDefaultVal();
                        //  detailMainKey.SetValue(x, keyValue);
                        SetDetailEntityValue<T, TDetail>(entity, x);
                        repository.AddWithSetIdentity<TDetail>(x);
                        //repository.DbContext.Entry<TDetail>(x).State = EntityState.Added;
                    });
                    // repository.DbContext.SaveChanges();
                }
                Response.OK(ResponseType.SaveSuccess);
                if (AddOnExecuted != null)
                    Response = AddOnExecuted(entity, list);
                return Response;
            });
            if (Response.Status && string.IsNullOrEmpty(Response.Message))
            {
                Response.OK(ResponseType.SaveSuccess);
            }
            AddProcese(entity);
            return Response;
        }

        /// <summary>
        /// 设置审批字段默认值
        /// </summary>
        /// <param name="entity"></param>
        private void SetAuditDefaultValue(T entity)
        {
            //if (!WorkFlowManager.Exists<T>())
            //{
            //    return;
            //}
            var propertity = TProperties.Where(x => x.Name.ToLower() == "auditstatus").FirstOrDefault();
            if (propertity != null && propertity.GetValue(entity) == null)
            {
                propertity.SetValue(entity, 0);
            }
        }
        ///// <summary>
        ///// 写入流程
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="entity"></param>
        ///// <param name="changeTableStatus">是否修改原表的审批状态</param>
        //protected void RewriteFlow(T entity, bool changeTableStatus = true)
        //{
        //    WorkFlowManager.AddProcese(entity, true, changeTableStatus);
        //}

        /// <summary>
        /// 获取三级明细表
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        private Type GetSubType(Type detailType)
        {
            return detailType.GetCustomAttribute<EntityAttribute>()?.DetailTable?[0];
        }

        public bool AddProcese(T entity)
        {
            if (!CheckResponseResult() && WorkFlowManager.Exists<T>(WorkFlowTableName))
            {
                if (AddWorkFlowExecuting != null && !AddWorkFlowExecuting.Invoke(entity))
                {
                    return false;
                }
                //写入流程
                WorkFlowManager.AddProcese<T>(entity, addWorkFlowExecuted: AddWorkFlowExecuted, workFlowTableName: WorkFlowTableName);
                return true;
                // WorkFlowManager.Audit<T>(entity, AuditStatus.待审批, null, null, null, null, init: true, initInvoke: AddWorkFlowExecuted);
            }
            return false;
        }
        /// <summary>
        /// 提交审批数据 2023.11.12
        /// </summary>
        /// <param name="id"></param>
        /// <param name="msg"></param>
        /// <param name="flowWriteState"></param>
        /// <returns></returns>
        public virtual WebResponseContent SubmitWorkFlowAudit(object[] ids)
        {
            Expression<Func<T, bool>> whereExpression = typeof(T).GetKeyName().CreateExpression<T>(ids, LinqExpressionType.In);
            var list = repository.FindAsIQueryable(whereExpression).ToList();
            if (list.Count != ids.Length)
            {
                return Response.Error($"未查到数据,或者数据已被删除");
            }
            var auditProperty = GetAuditProperty();
            if (auditProperty == null)
            {
                return Response.Error("表缺少审核状态字段：AuditStatus");
            }
            foreach (var entity in list)
            {
                int val = auditProperty.GetValue(entity).GetInt();
                if (val != (int)AuditStatus.草稿 && val != (int)AuditStatus.待提交)
                {
                    return Response.Error("只能提交[草稿]或[待提交]数据", true);
                }
            }
            Response = repository.DbContextBeginTransaction(() =>
            {
                foreach (var entity in list)
                {
                    WorkFlowManager.AddProcese<T>(entity, addWorkFlowExecuted: AddWorkFlowExecuted, checkId: true, workFlowTableName: WorkFlowTableName);
                    auditProperty.SetValue(entity, (int)AuditStatus.待审核);
                    repository.Update(entity, new string[] { auditProperty.Name });
                }
                repository.SaveChanges();
                return Response.OK("提交成功", true);
                //return 
            });
            return Response;
        }

        public void AddDetailToDBSet<TDetail>() where TDetail : class
        {
            List<PropertyInfo> listChilds = TProperties.Where(x => x.PropertyType.Name == "List`1").ToList();
            // repository.DbContext.Set<TDetail>().AddRange();
        }
        /// <summary>
        /// 一对多新建功能
        /// </summary>
        /// <param name="saveDataModel"></param>
        /// <returns></returns>
        private WebResponseContent AddMultipleDetail(SaveModel saveDataModel)
        {
            Response.OK();
            T mainEntity = saveDataModel.MainData.DicToEntity<T>();
            SetAuditDefaultValue(mainEntity);

            var tables = typeof(T).GetCustomAttribute<EntityAttribute>().DetailTable;
            for (int i = 0; i < saveDataModel.Details.Count; i++)
            {
                DetailInfo detailInfo = saveDataModel.Details[i];
                if (detailInfo.Data == null && detailInfo.Data.Count == 0)
                {
                    continue;
                }
                Type type = tables.Where(c => c.Name == detailInfo.Table).FirstOrDefault();
                if (type == null)
                {
                    return Response.Error($"未找到明细表[{detailInfo.Table}],请重新配置主表的明细表");
                }
                Type subType = GetSubType(type);
                string reslut = null;
                if (subType == null)
                {
                    //验证明细
                    reslut = type.ValidateDicInEntity(detailInfo.Data, true, false, new string[] { TProperties.GetKeyName() });
                }
                else
                {
                    //验证明细
                    reslut = type.ValidateDicInEntity(detailInfo.Data, true, false, new string[] { TProperties.GetKeyName(), subType.Name });
                    //验证三级明细表
                    if (string.IsNullOrEmpty(reslut))
                    {
                        ValidateSubDicInEntity(subType, detailInfo.Data);
                        if (!Response.Status)
                        {
                            return Response;
                        }
                    }
                }
                var logicDelProperty = GetLogicDelProperty(type);
                if (logicDelProperty != null)
                {
                    foreach (var item in detailInfo.Data)
                    {
                        item.TryAdd(logicDelProperty.Name, ((int)DelStatus.正常).ToString());
                    }
                }

                if (reslut != string.Empty)
                {
                    return Response.Error($"{type.GetEntityTableCnName()}:{reslut}");
                }
                //一对多设置明细表数据
                typeof(ServiceBase<T, TRepository>).GetMethod("SetEntityDetail", BindingFlags.Instance | BindingFlags.NonPublic)
                             .MakeGenericMethod(new Type[] { type })
                             .Invoke(this, new object[] { mainEntity, detailInfo.Data, null, subType != null });
            }
            mainEntity.SetTenancyValue().SetValue();
            mainEntity.CreateCode();
            if (AddOnExecuting != null)
            {
                Response = AddOnExecuting(mainEntity, null);
                if (CheckResponseResult()) return Response;
            }

            Response = repository.DbContextBeginTransaction(() =>
            {
                repository.AddWithSetIdentity(mainEntity);
                Type[] detailTypes = typeof(T).GetCustomAttribute<EntityAttribute>().DetailTable;
                foreach (var item in detailTypes)
                {
                    Type subType = item.GetCustomAttribute<EntityAttribute>()?.DetailTable?[0] ?? item;
                    typeof(ServiceBase<T, TRepository>).GetMethod("InsertNav", BindingFlags.Instance | BindingFlags.NonPublic)
                                 .MakeGenericMethod(new Type[] { item, subType })
                                 .Invoke(this, new object[] { mainEntity });
                }

                if (AddOnExecuted != null)
                    Response = AddOnExecuted(mainEntity, null);
                return Response;
            });
            if (Response.Status && string.IsNullOrEmpty(Response.Message))
            {
                Response.OK(ResponseType.SaveSuccess);
            }
            AddProcese(mainEntity);
            var propertyKey = typeof(T).GetKeyProperty();
            // saveDataModel.MainData[propertyKey.Name] = propertyKey.GetValue(mainEntity);
            Response.Data = new { data = mainEntity };
            return Response;
        }
        /// <summary>
        /// 设置主表的主键值
        /// </summary>
        /// <typeparam name="Main"></typeparam>
        /// <typeparam name="Detail"></typeparam>
        /// <param name="entity"></param>
        /// <param name="detailEntity"></param>
        private void SetEntityValue(T entity)
        {
            var mainType = typeof(T);
            var mainProperty = mainType.GetKeyProperty();
            if ((mainProperty.PropertyType == typeof(long) && AppSetting.UseSnow) || mainProperty.PropertyType == typeof(string))
            {
                var idWork = new Utilities.IdWorker();
                //adams 2023.11.8
                var att = mainProperty.GetCustomAttribute<MaxLengthAttribute>();
                if ((mainProperty.PropertyType == typeof(string)) && (att != null) && (att.Length == 38))
                {
                    mainProperty.SetValue(entity, (idWork.NextId()).ToString());
                    return;
                }
                else if (mainProperty.PropertyType == typeof(string))
                {
                    object value = mainProperty.GetValue(entity);
                    if (value?.ToString()?.Trim() == "")
                    {
                        mainProperty.SetValue(entity, Guid.NewGuid().ToString());
                    }
                    return;
                }
                else
                {

                    mainProperty.SetValue(entity, idWork.NextId());
                }
            }
        }

        /// <summary>
        /// 设置二三级表新建数据的主键值
        /// </summary>
        /// <typeparam name="Main"></typeparam>
        /// <typeparam name="Detail"></typeparam>
        /// <param name="entity"></param>
        /// <param name="detailEntity"></param>
        private void SetDetailEntityValue<Main, Detail>(Main entity, Detail detailEntity)
        {
            var detailProperty = typeof(Detail).GetKeyProperty();

            var mainType = typeof(Main);
            //主表主键字段
            var mainKeyValue = mainType.GetKeyProperty().GetValue(entity);

            var mainKeyPro = typeof(Detail).GetProperty(mainType.GetKeyProperty().Name);
            mainKeyPro.SetValue(detailEntity, mainKeyValue);

            if ((detailProperty.PropertyType == typeof(long) && AppSetting.UseSnow) || detailProperty.PropertyType == typeof(string))
            {
                //adams 2023.11.18
                var att = detailProperty.GetCustomAttribute<MaxLengthAttribute>();
                var idWork = new Utilities.IdWorker();
                if ((detailProperty.PropertyType == typeof(string)) && (att != null) && (att.Length == 38))
                {
                    detailProperty.SetValue(detailEntity, idWork.NextId().ToString());
                    return;
                }
                else if (detailProperty.PropertyType == typeof(string))
                {
                    detailProperty.SetValue(detailEntity, Guid.NewGuid().ToString());
                    return;
                }
                else
                {
                    detailProperty.SetValue(detailEntity, idWork.NextId());
                }
            }
        }

        /// <summary>
        /// 二级明细插入
        /// </summary>
        /// <typeparam name="Detail"></typeparam>
        /// <param name="insertNav"></param>
        /// <param name="name"></param>
        private void InsertNav<Detail, SubDetail>(T mainEntity)
          where Detail : class, new()
          where SubDetail : class, new()
        {
            var obj = typeof(T).GetProperty(typeof(Detail).Name).GetValue(mainEntity);
            if (obj == null)
            {
                return;
            }
            List<Detail> details = obj as List<Detail>;
            if (details == null || details.Count == 0)
            {
                return;
            }
            foreach (var detail in details)
            {
                SetDetailEntityValue<T, Detail>(mainEntity, detail);
                repository.AddWithSetIdentity<Detail>(detail);
                obj = typeof(Detail).GetProperty(typeof(SubDetail).Name)?.GetValue(detail);
                if (obj == null)
                {
                    continue;
                }
                List<SubDetail> subDetails = obj as List<SubDetail>;
                if (subDetails == null || subDetails.Count == 0)
                {
                    continue;
                }
                foreach (var sub in subDetails)
                {
                    SetDetailEntityValue<Detail, SubDetail>(detail, sub);
                    repository.AddWithSetIdentity<SubDetail>(sub);
                }
            }
        }

        /// <summary>
        /// 验证三级明细
        /// </summary>
        /// <param name="subType"></param>
        /// <param name="data"></param>
        private void ValidateSubDicInEntity(Type subType, List<Dictionary<string, object>> data)
        {
            if (!data.Any(x => x.ContainsKey(subType.Name)))
            {
                return;
            }
            var user = UserContext.Current.UserInfo;
            foreach (var item in data)
            {
                if (!item.TryGetValue(subType.Name, out object value) || value == null)
                {
                    continue;
                }
                List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                foreach (JObject jObject in (JArray)value)
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    foreach (var property in jObject.Properties())
                    {
                        dict[property.Name] = property.Value.ToObject<object>();
                    }
                    string result = subType.ValidateDicInEntity(dict, removeNotContains: false, removerKey: false);
                    if (!string.IsNullOrEmpty(result))
                    {
                        Response.Error($"{subType.GetEntityTableCnName()}:{result}");
                        return;
                    }
                    dict.TryAdd(AppSetting.CreateMember.UserIdField, user.User_Id);
                    dict.TryAdd(AppSetting.CreateMember.UserNameField, user.UserTrueName);
                    dict.TryAdd(AppSetting.CreateMember.DateField, DateTime.Now);
                    list.Add(dict);
                }
                item[subType.Name] = list;
            }

        }
        /// <summary>
        /// 一对多设置明细表数据
        /// </summary>
        /// <typeparam name="TDetail"></typeparam>
        /// <param name="entity"></param>
        /// <param name="detailData"></param>
        private void SetEntityDetail<TDetail>(T entity, List<Dictionary<string, object>> detailData, List<TDetail> list, bool hasSubDetail = false) where TDetail : class
        {
            bool setDefault = false;
            if (list == null)
            {
                setDefault = true;
                list = hasSubDetail
                         ? detailData.Serialize().DeserializeObject<List<TDetail>>()
                        : detailData.DicToList<TDetail>();
                PropertyInfo detailProperty = typeof(TDetail).GetKeyProperty();
                if (detailProperty.PropertyType == typeof(long) && AppSetting.UseSnow)
                {
                    var idWork = new Utilities.IdWorker();
                    var mainType = typeof(T);
                    var mainKeyValue = mainType.GetKeyProperty().GetValue(entity);
                    var mainKeyPro = typeof(TDetail).GetProperty(mainType.GetKeyProperty().Name);
                    foreach (var item in list)
                    {
                        detailProperty.SetValue(item, idWork.NextId());
                        mainKeyPro.SetValue(item, mainKeyValue);
                    }
                }
                else if (detailProperty.PropertyType == typeof(string))
                {
                    var mainType = typeof(T);
                    var mainKeyValue = mainType.GetKeyProperty().GetValue(entity);
                    var mainKeyPro = typeof(TDetail).GetProperty(mainType.GetKeyProperty().Name);
                    foreach (var item in list)
                    {
                        detailProperty.SetValue(item, Guid.NewGuid().ToString());
                        mainKeyPro.SetValue(item, mainKeyValue);
                    }
                }
            }
            PropertyInfo property = typeof(T).GetProperty(typeof(TDetail).Name);
            if (setDefault)
            {
                foreach (var item in list)
                {
                    //设置用户默认值
                    item.SetCreateDefaultVal();
                }
            }
            property.SetValue(entity, list);
        }

        /// <summary>
        /// 主从表新建
        /// </summary>
        /// <typeparam name="TDetail"></typeparam>
        /// <param name="saveDataModel"></param>
        /// <returns></returns>
        private WebResponseContent Add<TDetail>(SaveModel saveDataModel) where TDetail : class, new()
        {
            T mainEntity = saveDataModel.MainData.DicToEntity<T>();
            //验证明细
            string reslut = typeof(TDetail).ValidateDicInEntity(saveDataModel.DetailData, true, false, new string[] { TProperties.GetKeyName() });
            if (reslut != string.Empty)
                return Response.Error(reslut);

            List<TDetail> list = saveDataModel.DetailData.DicToList<TDetail>();
            Response = Add<TDetail>(mainEntity, list, false);

            //保存失败
            if (CheckResponseResult())
            {
                Logger.Error(LoggerType.Add, saveDataModel.Serialize() + Response.Message);
                return Response;
            }

            PropertyInfo propertyKey = typeof(T).GetKeyProperty();
            saveDataModel.MainData[propertyKey.Name] = propertyKey.GetValue(mainEntity);
            Response.Data = new { data = saveDataModel.MainData, list };
            return Response.Set(ResponseType.SaveSuccess);
        }

        #region 编辑

        /// <summary>
        /// 获取编辑明细主键
        /// </summary>
        /// <typeparam name="DetailT"></typeparam>
        /// <typeparam name="Tkey"></typeparam>
        /// <param name="detailKeyName"></param>
        /// <param name="mainKeyName"></param>
        /// <param name="mainKeyValue"></param>
        /// <returns></returns>
        public List<Tkey> GetUpdateDetailSelectKeys<DetailT, Tkey>(string detailKeyName, string mainKeyName, string mainKeyValue) where DetailT : class
        {
            ISugarQueryable<DetailT> queryable = repository.DbContext.Set<DetailT>();
            Expression<Func<DetailT, Tkey>> selectExpression = detailKeyName.GetExpression<DetailT, Tkey>();
            Expression<Func<DetailT, bool>> whereExpression = mainKeyName.CreateExpression<DetailT>(mainKeyValue, LinqExpressionType.Equal);
            List<Tkey> detailKeys = queryable.Where(whereExpression).Select(selectExpression).ToList();
            return detailKeys;
        }



        /// <summary>
        /// 获取配置的创建人ID创建时间创建人,修改人ID修改时间、修改人与数据相同的字段
        /// </summary>
        private static string[] _userIgnoreFields { get; set; }

        private static string[] UserIgnoreFields
        {
            get
            {
                if (_userIgnoreFields != null) return _userIgnoreFields;
                List<string> fields = new List<string>();
                //逻辑删除字段
                if (!string.IsNullOrEmpty(AppSetting.LogicDelField))
                {
                    fields.Add(AppSetting.LogicDelField);
                }
                fields.AddRange(CreateFields);
                fields.AddRange(ModifyFields);
                _userIgnoreFields = fields.ToArray();
                return _userIgnoreFields;
            }
        }
        private static string[] _createFields { get; set; }
        private static string[] CreateFields
        {
            get
            {
                if (_createFields != null) return _createFields;
                _createFields = AppSetting.CreateMember.GetType().GetProperties()
                    .Select(x => x.GetValue(AppSetting.CreateMember)?.ToString())
                    .Where(w => !string.IsNullOrEmpty(w)).ToArray();
                return _createFields;
            }
        }

        private static string[] _modifyFields { get; set; }
        private static string[] ModifyFields
        {
            get
            {
                if (_modifyFields != null) return _modifyFields;
                _modifyFields = AppSetting.ModifyMember.GetType().GetProperties()
                    .Select(x => x.GetValue(AppSetting.ModifyMember)?.ToString())
                    .Where(w => !string.IsNullOrEmpty(w)).ToArray();
                return _modifyFields;
            }
        }

        /// <summary>
        /// 编辑
        /// 1、明细表必须把主表的主键字段也设置为可编辑
        /// 2、修改、增加只会操作设置为编辑列的数据
        /// </summary>
        /// <param name="saveModel"></param>
        /// <returns></returns>
        public virtual WebResponseContent Update(SaveModel saveModel)
        {
            if (UpdateOnExecute != null)
            {
                Response = UpdateOnExecute(saveModel);
                if (CheckResponseResult()) return Response;
            }
            if (saveModel == null)
                return Response.Error(ResponseType.ParametersLack);


            //if (WorkFlowManager.Exists<T>())
            //{
            //    var auditProperty = TProperties.Where(x => x.Name.ToLower() == "auditstatus").FirstOrDefault();
            //    string value = saveModel.MainData[auditProperty.Name]?.ToString();
            //    if (WorkFlowManager.GetAuditStatus<T>(value) != 1)
            //    {
            //        return Response.Error("数据已经在审批中，不能修改");
            //    }
            //}
            Type type = typeof(T);

            //设置修改时间,修改人的默认值
            UserInfo userInfo = UserContext.Current.UserInfo;
            saveModel.SetDefaultVal(AppSetting.ModifyMember, userInfo);


            //判断提交的数据与实体格式是否一致
            string result = type.ValidateDicInEntity(saveModel.MainData, true, false, UserIgnoreFields, requireAllField: false);
            if (result != string.Empty)
                return Response.Error(result);

            PropertyInfo mainKeyProperty = type.GetKeyProperty();
            //adams 2023.4.29
            object keyDefaultVal = "";
            //获取主建类型的默认值用于判断后面数据是否正确,int long默认值为0,guid :0000-000....
            //object keyDefaultVal = mainKeyProperty.PropertyType.Assembly.CreateInstance(mainKeyProperty.PropertyType.FullName);
            if (mainKeyProperty.PropertyType != typeof(System.String))
                keyDefaultVal = mainKeyProperty.PropertyType.Assembly.CreateInstance(mainKeyProperty.PropertyType.FullName);
            //判断是否包含主键
            if (mainKeyProperty == null
                || !saveModel.MainData.ContainsKey(mainKeyProperty.Name)
                || saveModel.MainData[mainKeyProperty.Name] == null
                )
            {
                return Response.Error(ResponseType.NoKey);
            }

            object mainKeyVal = saveModel.MainData[mainKeyProperty.Name];
            //判断主键类型是否正确
            (bool, string, object) validation = mainKeyProperty.ValidationValueForDbType(mainKeyVal).FirstOrDefault();
            if (!validation.Item1)
                return Response.Error(ResponseType.KeyError);

            object valueType = mainKeyVal.ToString().ChangeType(mainKeyProperty.PropertyType);
            //判断主键值是不是当前类型的默认值
            if (valueType == null ||
                (!valueType.GetType().Equals(mainKeyProperty.PropertyType)
                || valueType.ToString() == keyDefaultVal.ToString()
                ))
                return Response.Error(ResponseType.KeyError);

            if (saveModel.MainData.Count <= 1) return Response.Error("系统没有配置好编辑的数据，请检查model或设置编辑行再点击生成model!");

            Expression<Func<T, bool>> expression = mainKeyProperty.Name.CreateExpression<T>(mainKeyVal.ToString(), LinqExpressionType.Equal);
            if (!repository.Exists(expression)) return Response.Error("保存的数据不存在!");

            saveModel.DetailData = saveModel.DetailData == null
                                         ? new List<Dictionary<string, object>>()
                                         : saveModel.DetailData.Where(x => x.Count > 0).ToList();

            //没有明细的直接保存主表数据
            if (!(saveModel.DetailData.Count > 0 || saveModel.DelKeys?.Count > 0 || (saveModel.Details != null && saveModel.Details.Count > 0)))
            {
                saveModel.SetDefaultVal(AppSetting.ModifyMember, userInfo);

                T mainEntity = saveModel.MainData.DicToEntity<T>();
                CheckDataVersion(mainEntity, saveModel);
                if (CheckResponseResult())
                {
                    return Response;
                }
                if (UpdateOnExecuting != null)
                {
                    Response = UpdateOnExecuting(mainEntity, null, null, null);
                    if (CheckResponseResult()) return Response;
                }
                //不修改!CreateFields.Contains创建人信息
                repository.Update(mainEntity, type.GetEditField().Where(c => saveModel.MainData.Keys.Contains(c) && !CreateFields.Contains(c)).ToArray());
                if (base.UpdateOnExecuted == null)
                {
                    repository.SaveChanges();
                    Response.OK(ResponseType.SaveSuccess);
                }
                else
                {
                    Response = repository.DbContextBeginTransaction(() =>
                    {
                        repository.SaveChanges();
                        Response = UpdateOnExecuted(mainEntity, null, null, null);
                        return Response;
                    });
                }
                if (Response.Status) Response.Data = new { data = mainEntity };
                if (Response.Status && string.IsNullOrEmpty(Response.Message))
                    Response.OK(ResponseType.SaveSuccess);
                return Response;
            }

            return UpdateMultipleDetail(saveModel, mainKeyProperty, keyDefaultVal);
        }

        #endregion

        /// <summary>
        /// 提交的数据版号检测
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="saveModel"></param>
        private void CheckDataVersion(T entity, SaveModel saveModel)
        {
            Response.Status = true;
            if (string.IsNullOrEmpty(saveModel.DataVersionField) || string.IsNullOrEmpty(saveModel.DataVersionValue))
            {
                return;
            }
            var versionProperty = typeof(T).GetProperty(saveModel.DataVersionField);
            if (versionProperty == null)
            {
                return;
            }
            var keyProperty = typeof(T).GetKeyProperty();
            object keyValue = keyProperty.GetValue(entity);
            var where = keyProperty.Name.CreateExpression<T>(keyValue, LinqExpressionType.Equal);
            var selectExpression = saveModel.DataVersionField.GetExpression<T, string>();
            string dataVersionValue = repository.FindAsIQueryable(where).Select(selectExpression).FirstOrDefault();
            if (string.IsNullOrEmpty(dataVersionValue))
            {
                return;
            }
            if (dataVersionValue != saveModel.DataVersionValue)
            {
                Response.Error("数据已发生变化,请刷新页面后重新编辑".Translator());
            }
            string value = Guid.NewGuid().ToString();
            versionProperty.SetValue(entity, value);
            saveModel.MainData[saveModel.DataVersionField] = value;
        }



        /// <summary>
        /// 一对多编辑功能
        /// </summary>
        /// <param name="saveDataModel"></param>
        /// <returns></returns>
        private WebResponseContent UpdateMultipleDetail(SaveModel saveModel, PropertyInfo mainKeyProperty, object keyDefaultVal)
        {
            Response.OK();
            bool isMultipleDetail = false;
            var entity = saveModel.MainData.DicToEntity<T>();
            CheckDataVersion(entity, saveModel);
            if (CheckResponseResult())
            {
                return Response;
            }
            List<DetailInfo> details = new List<DetailInfo>();
            //一对多细明
            if (saveModel.Details != null && saveModel.Details.Count > 0)
            {
                details = saveModel.Details;
                isMultipleDetail = true;
                // return UpdateMultipleDetail(saveModel);
            }
            else
            {
                //主从明细
                Type detailType = GetRealDetailType();
                details.Add(new DetailInfo()
                {
                    Data = saveModel.DetailData,
                    DelKeys = saveModel.DelKeys,
                    //adams 2023.11.18 修改中表别名的不兼容
                    //Table = detailType.GetEntityTableName()
                    Table = detailType.Name
                });
            }

            CreateSubDel(saveModel);

            //判断明细是否包含了主表的主键
            var detailTypes = typeof(T).GetCustomAttribute<EntityAttribute>().DetailTable;

            int index = 0;
            foreach (var item in details)
            {
                var detailType = detailTypes.Where(c => c.Name == item.Table).FirstOrDefault();
                var detailKeyInfo = detailType.GetKeyProperty();


                //adams 2023.10.31
                string deatilDefaultVal;
                if (detailKeyInfo.PropertyType == typeof(string))
                    deatilDefaultVal = "";
                else
                    deatilDefaultVal = detailKeyInfo.PropertyType.Assembly.CreateInstance(detailKeyInfo.PropertyType.FullName).ToString();

                string result = detailType.ValidateDicInEntity(item.Data, false, false);
                if (!string.IsNullOrEmpty(result))
                {
                    return Response.Error($"{detailType.GetEntityTableCnName()}:{result}");
                }

                Type subType = GetSubType(detailType);
                if (subType != null)
                {
                    //验证三级明细表
                    ValidateSubDicInEntity(subType, item.Data);
                    if (!Response.Status)
                    {
                        return Response;
                    }
                }

                foreach (Dictionary<string, object> dic in item.Data)
                {
                    //不包含主键的默认添加主键默认值，用于后面判断是否为新增数据
                    if (!dic.ContainsKey(detailKeyInfo.Name))
                    {
                        dic.Add(detailKeyInfo.Name, keyDefaultVal);
                        if (dic.ContainsKey(mainKeyProperty.Name))
                        {
                            dic[mainKeyProperty.Name] = keyDefaultVal;
                        }
                        else
                        {
                            dic.Add(mainKeyProperty.Name, keyDefaultVal);
                        }
                        continue;
                    }
                    if (dic[detailKeyInfo.Name] == null)
                        return Response.Error(ResponseType.NoKey);

                    //主键值是否正确
                    string detailKeyVal = dic[detailKeyInfo.Name].ToString();
                    if (detailKeyInfo.PropertyType != typeof(string) && (!detailKeyInfo.ValidationValueForDbType(detailKeyVal).FirstOrDefault().Item1
                        || deatilDefaultVal == detailKeyVal))
                        return Response.Error(ResponseType.KeyError);

                    //判断主表的值是否正确
                    if (detailKeyVal != keyDefaultVal.ToString() && (!dic.ContainsKey(mainKeyProperty.Name) || dic[mainKeyProperty.Name] == null || dic[mainKeyProperty.Name].ToString() == keyDefaultVal.ToString()))
                    {
                        return Response.Error($"[{mainKeyProperty.Name.Translator()}]{"是必填项".Translator()}");
                    }
                    if (saveModel.DetailData.Exists(c => c.Count <= 2))
                    {
                        return Response.Error("系统没有配置好明细编辑的数据，请检查model,或者明细表点击生成model!");
                    }

                }
                index++;
                this.GetType().GetMethod("UpdateToEntity")
                  .MakeGenericMethod(new Type[] { detailType })
                  .Invoke(this, new object[] {
                       entity,
                       saveModel.MainData,
                       item.Data,
                       item.DelKeys,
                       mainKeyProperty,
                       detailKeyInfo,
                       keyDefaultVal,
                       index == details.Count,
                       isMultipleDetail,
                       subType
                  });
            }

            return Response;
        }

        private List<MultipleTableEntity> multipleTableEntities = null;


        /// <summary>
        /// 将数据转换成对象后最终保存
        /// </summary>
        /// <typeparam name="DetailT"></typeparam>
        /// <param name="saveModel"></param>
        /// <param name="mainKeyProperty"></param>
        /// <param name="detailKeyInfo"></param>
        /// <param name="keyDefaultVal"></param>
        /// <returns></returns>
        public void UpdateToEntity<DetailT>(
                T mainEntity,
                Dictionary<string, object> mainData,
                List<Dictionary<string, object>> detailData,
                List<object> detailDelKeys,
                PropertyInfo mainKeyProperty,
                PropertyInfo detailKeyInfo,
                object keyDefaultVal,
                bool save,
                bool isMultipleDetail,
                Type subType
                ) where DetailT : class
        {
            if (multipleTableEntities == null)
            {
                multipleTableEntities = new List<MultipleTableEntity>();
            }
            // T mainEntity = saveModel.MainData.DicToEntity<T>();
            List<DetailT> detailList = subType != null
                ? detailData.Serialize().DeserializeObject<List<DetailT>>()
               : detailData.DicToList<DetailT>();

            //新增对象
            List<DetailT> addList = new List<DetailT>();
            //   List<object> containsKeys = new List<object>();
            //编辑对象
            List<DetailT> editList = new List<DetailT>();
            //删除的主键
            List<object> delKeys = new List<object>();
            Type detailType = typeof(DetailT);
            mainKeyProperty = detailType.GetProperties().Where(x => x.Name == mainKeyProperty.Name).FirstOrDefault();
            //获取新增与修改的对象
            foreach (DetailT item in detailList)
            {
                object value = detailKeyInfo.GetValue(item);
                if (keyDefaultVal.Equals(value))//主键为默认值的,新增数据
                {
                    //设置新增的主表的值
                    //mainKeyProperty.SetValue(item,
                    //    mainData[mainKeyProperty.Name]
                    //    .ChangeType(mainKeyProperty.PropertyType));

                    //if (detailKeyInfo.PropertyType == typeof(Guid))
                    //{
                    //    detailKeyInfo.SetValue(item, Guid.NewGuid());
                    //}
                    SetDetailEntityValue<T, DetailT>(mainEntity, item);
                    addList.Add(item);
                }
                else //if (detailKeys.Contains(value))
                {
                    editList.Add(item);
                }
            }

            //获取需要删除的对象的主键
            if (detailDelKeys != null && detailDelKeys.Count > 0)
            {
                //2021.08.21优化明细表删除
                delKeys = detailDelKeys.Select(q => q.ChangeType(detailKeyInfo.PropertyType)).Where(x => x != null).ToList();
                //.Where(x => detailKeys.Contains(x.ChangeType(detailKeyInfo.PropertyType)))
                //.Select(q => q.ChangeType(detailKeyInfo.PropertyType)).ToList();
            }

            if (editList.Count > 0)
            {
                //明细修改
                editList.ForEach(x =>
                {
                    //設置默認值
                    x.SetModifyDefaultVal();
                    //添加修改字段

                    // repository.Update<DetailT>(x, updateField.ToArray());
                });
                //获取编辑的字段
                var updateField = detailData
                    .Where(c => c[detailKeyInfo.Name].ChangeType(detailKeyInfo.PropertyType)
                    .Equal(detailKeyInfo.GetValue(editList[0])))
                    .FirstOrDefault()
                    .Keys.Where(k => k != detailKeyInfo.Name)
                    .Where(r => !CreateFields.Contains(r) && r != subType?.Name)
                    .ToList();
                updateField.AddRange(ModifyFields);
                //if (subType != null)
                //{
                //    updateField.Add(subType.Name);
                //}
                multipleTableEntities.Add(new MultipleTableEntity()
                {
                    Type = detailType,
                    List = editList,
                    Flag = TableFlag.Update,
                    Fields = updateField,
                    SubType = subType
                });
            }


            if (addList.Count > 0)
            {
                var idWork = new Utilities.IdWorker();
                if (detailKeyInfo.PropertyType == typeof(long) && AppSetting.UseSnow)
                {
                    foreach (var item in addList)
                    {
                        detailKeyInfo.SetValue(item, idWork.NextId());
                    }
                }
                else if (detailKeyInfo.PropertyType == typeof(string))
                {
                    //adams 2023.11.18 雪花算法
                    var att = detailKeyInfo.GetCustomAttribute<MaxLengthAttribute>();

                    foreach (var item in addList)
                    {
                        if ((detailKeyInfo.PropertyType == typeof(string)) && (att != null) && (att.Length == 38))
                            detailKeyInfo.SetValue(item, idWork.NextId().ToString());
                        else
                            detailKeyInfo.SetValue(item, Guid.NewGuid().ToString());
                    }
                }
                PropertyInfo lgProperty = detailType.GetProperty(AppSetting.LogicDelField);
                //明细新增
                addList.ForEach(x =>
                {
                    x.SetCreateDefaultVal();

                    if (lgProperty != null)
                    {
                        lgProperty.SetValue(x, ((int)DelStatus.正常).ChangeType(lgProperty.PropertyType));
                    }

                    // repository.DbContext.Entry<DetailT>(x).State = EntityState.Added;
                });

                multipleTableEntities.Add(new MultipleTableEntity()
                {
                    Type = detailType,
                    List = addList,
                    Flag = TableFlag.Add,
                    SubType = subType
                });
            }

            //明细删除
            if (delKeys.Count > 0)
            {
                //delKeys.ForEach(x =>
                //{
                //    DetailT delT = Activator.CreateInstance<DetailT>();
                //    detailKeyInfo.SetValue(delT, x);
                //    repository.DbContext.Entry<DetailT>(delT).State = EntityState.Deleted;
                //});

                multipleTableEntities.Add(new MultipleTableEntity()
                {
                    Type = detailType,
                    List = delKeys,
                    Flag = TableFlag.Del
                });
            }

            //多个明细表时，记录明细表数据
            //一对多设置明细表数据
            if (isMultipleDetail)
            {
                var list = new List<DetailT>();
                list.AddRange(editList);
                list.AddRange(addList);
                typeof(ServiceBase<T, TRepository>).GetMethod("SetEntityDetail", BindingFlags.Instance | BindingFlags.NonPublic)
                             .MakeGenericMethod(new Type[] { detailType })
                             .Invoke(this, new object[] { mainEntity, null, list, false });
            }

            //不保存直接返回
            if (!save)
            {
                return;
            }

            //2023.07.11调整更新前方法
            if (UpdateOnExecuting != null)
            {
                //多明细表从mainEntity与SaveModel.Details中取数据
                //mainEntity为主表与明细表数据，SaveModel.Details中取删除的数据
                if (isMultipleDetail)
                {
                    Response = UpdateOnExecuting(mainEntity, null, null, null);
                }
                else
                {
                    Response = UpdateOnExecuting(mainEntity, addList, editList, delKeys);
                    if (addList.Count > 0 && !multipleTableEntities.Exists(x => x.Flag == TableFlag.Add))
                    {
                        multipleTableEntities.Add(new MultipleTableEntity()
                        {
                            Type = detailType,
                            List = addList,
                            Flag = TableFlag.Add
                        });
                    }

                    if (editList.Count > 0 && !multipleTableEntities.Exists(x => x.Flag == TableFlag.Update))
                    {
                        var updateField = detailData.SelectMany(x => x.Keys).Distinct().Where(k => k != detailKeyInfo.Name)
                              .Where(r => !CreateFields.Contains(r) && r != subType?.Name)
                              .ToList();
                        updateField.AddRange(ModifyFields);
                        multipleTableEntities.Add(new MultipleTableEntity()
                        {
                            Type = detailType,
                            List = editList,
                            Fields = updateField,
                            Flag = TableFlag.Update
                        });
                    }
                }
                if (CheckResponseResult())
                {
                    return;
                }
            }


            //最后一个明细执行保存


            //if (UpdateOnExecuted == null)
            //{
            //    SetMultipleTableEntities();
            //    repository.DbContext.SaveChanges();
            //    Response.OK(ResponseType.SaveSuccess);
            //}
            //else
            //{
            Response = repository.DbContextBeginTransaction(() =>
            {
                mainEntity.SetModifyDefaultVal();
                //主表修改
                //不修改!CreateFields.Contains创建人信息
                repository.Update(mainEntity, typeof(T).GetEditField()
                    .Where(c => mainData.Keys.Contains(c) && !CreateFields.Contains(c))
                    .ToArray(), true);

                SetMultipleTableEntities();
                repository.DbContext.SaveChanges();

                //多明细表从mainEntity与SaveModel.Details中取数据
                //mainEntity为主表与明细表数据，SaveModel.Details中取删除的数据
                if (UpdateOnExecuted != null)
                {
                    if (isMultipleDetail)
                    {
                        Response = UpdateOnExecuted(mainEntity, null, null, null);
                    }
                    else
                    {
                        Response = UpdateOnExecuted(mainEntity, addList, editList, delKeys);
                    }
                }
                return Response;
            });
            //  }
            if (Response.Status)
            {
                addList.AddRange(editList);
                Response.Data = new { data = mainEntity, list = addList };
                if (string.IsNullOrEmpty(Response.Message))
                    Response.OK(ResponseType.SaveSuccess);
            }
        }
        /// <summary>
        /// 2023.09.10增加主从、一对多编辑时可修改原对象属性
        /// </summary>
        private void SetMultipleTableEntities()
        {
            if (multipleTableEntities == null || multipleTableEntities.Count == 0)
            {
                return;
            }
            foreach (var item in multipleTableEntities)
            {
                typeof(ServiceBase<T, TRepository>).GetMethod("EntryDbContextMultipleTableEntities", BindingFlags.Instance | BindingFlags.NonPublic)
                             .MakeGenericMethod(new Type[] { item.Type })
                             .Invoke(this, new object[] { item });
            }
        }
        /// <summary>
        /// 2023.09.10增加主从、一对多编辑时可修改原对象属性
        /// </summary>
        /// <typeparam name="Detail"></typeparam>
        /// <param name="multipleTable"></param>
        private void EntryDbContextMultipleTableEntities<Detail>(MultipleTableEntity multipleTable) where Detail : class, new()
        {
            if (multipleTable.Flag == TableFlag.Add)
            {
                foreach (var detail in (List<Detail>)multipleTable.List)
                {
                    detail.SetCreateDefaultVal();
                    repository.AddWithSetIdentity<Detail>(detail);
                }
                if (multipleTable.SubType != null)
                {
                    typeof(ServiceBase<T, TRepository>).GetMethod("EntryDbContextSubType", BindingFlags.Instance | BindingFlags.NonPublic)
                       .MakeGenericMethod(new Type[] { multipleTable.SubType, typeof(Detail) })
                       .Invoke(this, new object[] { multipleTable.List });
                }
                return;
            }

            if (multipleTable.Flag == TableFlag.Update)
            {
                repository.UpdateRange((List<Detail>)multipleTable.List, multipleTable.Fields.ToArray());
                if (multipleTable.SubType != null)
                {
                    // 设置三级明细表
                    typeof(ServiceBase<T, TRepository>).GetMethod("EntryDbContextSubType", BindingFlags.Instance | BindingFlags.NonPublic)
                          .MakeGenericMethod(new Type[] { multipleTable.SubType, typeof(Detail) })
                          .Invoke(this, new object[] { multipleTable.List });
                }
                //repository.DbContext.Entry<Detail>(x).State = EntityState.Added;

                return;
            }
            var detailKeyInfo = typeof(Detail).GetKeyProperty();
            foreach (var key in (List<object>)multipleTable.List)
            {
                Detail delT = Activator.CreateInstance<Detail>();
                detailKeyInfo.SetValue(delT, key);
                repository.Delete<Detail>(delT, false);
                //  repository.DbContext.Entry<Detail>(delT).State = EntityState.Deleted;
            }
        }
        /// <summary>
        /// 设置三级明细表
        /// </summary>
        /// <typeparam name="TSub"></typeparam>
        /// <typeparam name="Detail"></typeparam>
        /// <param name="list"></param>
        private void EntryDbContextSubType<TSub, Detail>(List<Detail> list) where TSub : class, new()
        {
            string subTableName = typeof(TSub).Name;
            var subKeyPeroperty = typeof(TSub).GetKeyProperty();

            var subProperty = typeof(Detail).GetProperty(subTableName);
            var updateFields = typeof(TSub).GetProperties().Where(x =>
                x.Name != subKeyPeroperty.Name
                && x.Name != subProperty.Name
                && !CreateFields.Contains(x.Name))
                .Select(s => s.Name);
            foreach (var item in list)
            {
                var obj = subProperty.GetValue(item);
                if (obj == null)
                {
                    continue;
                }

                foreach (var sub in (List<TSub>)obj)
                {
                    string value = subKeyPeroperty.GetValue(sub)?.ToString();

                    if (string.IsNullOrEmpty(value) || value == "0" || value == Guid.Empty.ToString())
                    {
                        sub.SetCreateDefaultVal();
                        //  repository.Add(sub);
                        //新加的数据
                        SetDetailEntityValue<Detail, TSub>(item, sub);
                        repository.AddWithSetIdentity<TSub>(sub);
                    }
                    else
                    {
                        sub.SetModifyDefaultVal();
                        repository.Update<TSub>(sub, updateFields.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// 三级明细删除
        /// </summary>
        private void CreateSubDel(SaveModel saveModel)
        {
            if (saveModel.SubDelInfo == null || saveModel.SubDelInfo.Count == 0)
            {
                return;
            }

            var detailTypes = typeof(T).GetCustomAttribute<EntityAttribute>()?.DetailTable;
            if (detailTypes == null)
            {
                return;
            }

            foreach (var item in detailTypes)
            {
                var subTypes = item.GetCustomAttribute<EntityAttribute>()?.DetailTable;
                if (subTypes != null)
                {
                    foreach (var type in subTypes)
                    {
                        typeof(ServiceBase<T, TRepository>).GetMethod("CreateSubDelContext", BindingFlags.Instance | BindingFlags.NonPublic)
                             .MakeGenericMethod(new Type[] { type })
                             .Invoke(this, new object[] { saveModel, type.Name });
                    }

                }
            }
        }
        /// <summary>
        /// 三级表删除
        /// </summary>
        /// <typeparam name="TSub"></typeparam>
        /// <param name="saveModel"></param>
        /// <param name="tableName"></param>
        private void CreateSubDelContext<TSub>(SaveModel saveModel, string tableName) where TSub : class, new()
        {
            foreach (var item in saveModel.SubDelInfo.Where(x => !x.IsProescc && x.Table == tableName))
            {
                item.IsProescc = true;
                var keyPro = typeof(TSub).GetKeyProperty();
                foreach (var key in item.DelKeys)
                {
                    TSub entity = Activator.CreateInstance<TSub>();
                    keyPro.SetValue(entity, key.ChangeType(keyPro.PropertyType));
                    // repository.DbContext.Entry<TSub>(entity).State = EntityState.Deleted;
                    repository.Delete(entity);
                }

            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="delList">是否删除明细数据(默认会删除明细)</param>
        /// <returns></returns>
        public virtual WebResponseContent Del(object[] keys, bool delList = true)
        {
            Type entityType = typeof(T);
            var keyProperty = entityType.GetKeyProperty();
            if (keyProperty == null || keys == null || keys.Length == 0) return Response.Error(ResponseType.NoKeyDel);

            IEnumerable<(bool, string, object)> validation = keyProperty.ValidationValueForDbType(keys);
            if (validation.Any(x => !x.Item1))
            {
                return Response.Error(validation.Where(x => !x.Item1).Select(s => s.Item2 + "</br>").Serialize());
            }
            string tKey = keyProperty.Name;
            if (string.IsNullOrEmpty(tKey))
                return Response.Error("没有主键不能删除");

            if (DelOnExecuting != null)
            {
                Response = DelOnExecuting(keys);
                if (CheckResponseResult()) return Response;
            }
            PropertyInfo lgProperty = entityType.GetProperty(AppSetting.LogicDelField);
            //逻辑删除
            if (lgProperty != null)
            {
                LogicDel(keys, keyProperty, lgProperty);
                return Response;
            }
            // FieldType fieldType = entityType.GetFieldType();
            //string joinKeys = (fieldType == FieldType.Int || fieldType == FieldType.BigInt)
            //     ? string.Join(",", keys)
            //     : $"'{string.Join("','", keys)}'";

            // 2020.08.15添加判断多租户数据（删除）
            if (IsMultiTenancy && !UserContext.Current.IsSuperAdmin)
            {
                // CheckDelMultiTenancy(joinKeys, tKey);
                if (CheckResponseResult())
                {
                    return Response;
                }
            }

            var logicDelProperty = GetLogicDelProperty<T>();
            //逻辑删除
            if (logicDelProperty != null)
            {
                return LogicDel(keys, keyProperty, logicDelProperty);
            }
            //可能在删除后还要做一些其它数据库新增或删除操作，这样就可能需要与删除保持在同一个事务中处理
            Response = repository.DbContextBeginTransaction(() =>
            {
                //if (typeof(T).GetSugarSplitTable() != null)
                //{
                //    typeof(T).GetProperties().Where(x => x.GetConfigValue<SplitFieldAttribute>() != null)
                //    .FirstOrDefault()?.GetValue();

                //    repository.SqlSugarClient.SplitHelper<T>().GetTableName();
                //    repository.SqlSugarClient.Deleteable<T>().In(keys).SplitTable().ExecuteCommand();
                //}
                //else {
                repository.SqlSugarClient.Deleteable<T>().In(keys).ExecuteCommand();
                //}

                if (delList)
                {
                    var types = entityType.GetCustomAttribute<EntityAttribute>()?.DetailTable;
                    if (types != null)
                    {
                        foreach (var type in types)
                        {
                            typeof(ServiceBase<T, TRepository>)
                            .GetMethod("DelDetails", BindingFlags.Instance | BindingFlags.NonPublic)
                           .MakeGenericMethod(new Type[] { type, keyProperty.PropertyType })
                           .Invoke(this, new object[] { keys });
                        }
                    }
                }
                if (DelOnExecuted != null)
                {
                    Response = DelOnExecuted(keys);
                }
                return Response;
            });
            if (Response.Status && string.IsNullOrEmpty(Response.Message)) Response.OK(ResponseType.DelSuccess);
            return Response;
        }

        private void DelDetails<Entity, KeyType>(object[] keys) where Entity : class, new()
        {
            var values = keys.Select(s => (KeyType)(s.ChangeType(typeof(KeyType)))).ToList();
            var expression = typeof(T).GetKeyName().CreateExpression<Entity>(values, LinqExpressionType.In);
            repository.DbContext.Deleteable<Entity>().Where(expression).ExecuteCommand();
        }
        private object[] GetLogicDelIds<TKey>(object[] keys, PropertyInfo keyProperty)
        {
            var keyCondition = keyProperty.Name.CreateExpression<T>(keys, LinqExpressionType.In);
            var selectExp = keyProperty.Name.GetExpression<T, TKey>();
            return repository.FindAsIQueryable(keyCondition).Select(selectExp).ToArray().Select(s => s as object).ToArray();
        }
        /// <summary>
        /// 明细表逻辑删除
        /// </summary>
        /// <typeparam name="Entity"></typeparam>
        /// <typeparam name="KeyType"></typeparam>
        /// <param name="keys"></param>
        private void LogicDelDetails<Entity, KeyType>(object[] keys, PropertyInfo keyProperty) where Entity : class, new()
        {
            var values = keys.Select(s => (KeyType)(s.ChangeType(typeof(KeyType)))).ToList();
            var expression = keyProperty.Name.CreateExpression<Entity>(values, LinqExpressionType.In);

            repository.SqlSugarClient.Updateable<Entity>()
            .SetColumns(AppSetting.LogicDelField, (int)DelStatus.已删除)
            .Where(expression)
            .ExecuteCommand();
        }

        /// <summary>
        /// 逻辑删除
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="keyProperty"></param>
        /// <param name="logicDelProperty"></param>
        /// <returns></returns>
        private WebResponseContent LogicDel(object[] keys, PropertyInfo keyProperty, PropertyInfo logicDelProperty)
        {
            keys = typeof(ServiceBase<T, TRepository>).GetMethod("GetLogicDelIds", BindingFlags.Instance | BindingFlags.NonPublic)
                         .MakeGenericMethod(new Type[] { keyProperty.PropertyType })
                         .Invoke(this, new object[] { keys, keyProperty }) as object[];
            List<T> delList = new List<T>();

            foreach (object key in keys)
            {
                var entity = Activator.CreateInstance<T>();
                keyProperty.SetValue(entity, key.ChangeType(keyProperty.PropertyType));
                logicDelProperty.SetValue(entity, ((int)DelStatus.已删除).ChangeType(logicDelProperty.PropertyType));
                delList.Add(entity);
            }
            Response = repository.DbContextBeginTransaction(() =>
            {
                repository.UpdateRange(delList, new string[] { logicDelProperty.Name }, true);

                var types = typeof(T).GetCustomAttribute<EntityAttribute>()?.DetailTable;
                if (types != null && types.Length > 0)
                {
                    foreach (var detailType in types)
                    {
                        var logPro = GetLogicDelProperty(detailType);
                        if (logPro != null)
                        {
                            typeof(ServiceBase<T, TRepository>)
                                         .GetMethod("LogicDelDetails", BindingFlags.Instance | BindingFlags.NonPublic)
                                        .MakeGenericMethod(new Type[] { detailType, keyProperty.PropertyType })
                                        .Invoke(this, new object[] { keys, keyProperty });
                        }
                    }
                }
                if (DelOnExecuted != null)
                {
                    return DelOnExecuted(keys);
                }
                return Response.OK("删除成功".Translator());
            });
            return Response;
        }


        private static string[] auditFields = new string[] { "auditid", "auditstatus", "auditor", "auditdate", "auditreason" };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">表数据id</param>
        /// <param name="msg">重写流程的日志信息</param>
        /// <param name="flowWriteState">重新开始或回退到上一级节点</param>
        /// <returns></returns>
        public virtual WebResponseContent RestartWorkFlowAudit(object id, string msg, FlowWriteState flowWriteState = FlowWriteState.重新开始)
        {
            Expression<Func<T, bool>> whereExpression = typeof(T).GetKeyName().CreateExpression<T>(id, LinqExpressionType.Equal);
            T entity = repository.FindAsIQueryable(whereExpression).FirstOrDefault();
            if (entity == null)
            {
                return Response.Error($"未查到数据,或者数据已被删除,id:{id}");
            }
            Response = repository.DbContextBeginTransaction(() =>
            {
                return WorkFlowManager.Audit<T>(repository.BaseDbContext, entity, AuditStatus.审核未通过, msg, autditProperty: GetAuditProperty(), flowWriteState: flowWriteState, workFlowTableName: WorkFlowTableName);
            });
            return Response;
        }

        private PropertyInfo GetAuditProperty()
        {
            return TProperties.Where(x => x.Name.ToLower() == "auditstatus").FirstOrDefault();
        }
        /// <summary>
        /// 审批实际使用的表名(解决视图新建数据时实时调用原表新建进入不了流程的问题2024.01.01)
        /// </summary>
        public string WorkFlowTableName { get; set; }
        /// <summary>
        /// 审核默认对应数据库字段为AuditId审核人ID ,AuditStatus审核状态,Auditor审核人,Auditdate审核时间,Auditreason审核原因
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="auditStatus"></param>
        /// <param name="auditReason"></param>
        /// <returns></returns>
        public virtual WebResponseContent Audit(object[] keys, int? auditStatus, string auditReason)
        {
            if (keys == null || keys.Length == 0)
                return Response.Error("未获取到参数!");

            Expression<Func<T, bool>> whereExpression = typeof(T).GetKeyName().CreateExpression<T>(keys[0], LinqExpressionType.Equal);
            T entity = repository.FindAsIQueryable(whereExpression).FirstOrDefault();
            if (entity == null)
            {
                return Response.Error($"未查到数据,或者数据已被删除,id:{keys[0]}");
            }
            var auditProperty = GetAuditProperty();
            // var auditProperty = TProperties.Where(x => x.Name.ToLower() == "auditstatus").FirstOrDefault();
            if (auditProperty == null)
            {
                return Response.Error("表缺少审核状态字段：AuditStatus");
            }
            //进入流程审批
            if (WorkFlowManager.Exists<T>(entity))
            {
                AuditStatus status = (AuditStatus)Enum.Parse(typeof(AuditStatus), auditStatus.ToString());
                int val = auditProperty.GetValue(entity).GetInt();
                if (!(val == (int)AuditStatus.待审核 || val == (int)AuditStatus.审核中))
                {
                    return Response.Error("只能审批[待审核或审核中]的数据");
                }
                Response = repository.DbContextBeginTransaction(() =>
                {
                    return WorkFlowManager.Audit<T>(repository.BaseDbContext,
                        entity, status, auditReason,
                        auditProperty, AuditWorkFlowExecuting, AuditWorkFlowExecuted,
                        workFlowTableName: WorkFlowTableName);
                });
                if (Response.Status)
                {
                    return Response.OK(ResponseType.AuditSuccess);
                }
                return Response.Error(Response.Message ?? "审批失败");
            }


            //获取主键
            PropertyInfo property = TProperties.GetKeyProperty();
            if (property == null)
                return Response.Error("没有配置好主键!");

            UserInfo userInfo = UserContext.Current.UserInfo;

            //表如果有审核相关字段，设置默认审核

            PropertyInfo[] updateFileds = TProperties.Where(x => auditFields.Contains(x.Name.ToLower())).ToArray();
            List<T> auditList = new List<T>();
            foreach (var value in keys)
            {
                object convertVal = value.ToString().ChangeType(property.PropertyType);
                if (convertVal == null) continue;

                entity = Activator.CreateInstance<T>();
                property.SetValue(entity, convertVal);
                foreach (var item in updateFileds)
                {
                    switch (item.Name.ToLower())
                    {
                        case "auditid":
                            item.SetValue(entity, userInfo.User_Id);
                            break;
                        case "auditstatus":
                            item.SetValue(entity, auditStatus);
                            break;
                        case "auditor":
                            item.SetValue(entity, userInfo.UserTrueName);
                            break;
                        case "auditdate":
                            item.SetValue(entity, DateTime.Now);
                            break;
                        case "auditreason":
                            item.SetValue(entity, auditReason);
                            break;
                    }
                }
                auditList.Add(entity);
            }
            if (base.AuditOnExecuting != null)
            {
                Response = AuditOnExecuting(auditList);
                if (CheckResponseResult()) return Response;
            }
            Response = repository.DbContextBeginTransaction(() =>
            {
                //2023.11.26增加审批日志
                WorkFlowManager.AddAuditLog<T>(keys, auditStatus, auditReason);
                repository.UpdateRange(auditList, updateFileds.Select(x => x.Name).ToArray(), true);
                if (base.AuditOnExecuted != null)
                {
                    Response = AuditOnExecuted(auditList);
                    if (CheckResponseResult()) return Response;
                }
                return Response.OK();
            });
            if (Response.Status)
            {
                return Response.OK(ResponseType.AuditSuccess);
            }
            return Response.Error(Response.Message);
        }


        /// <summary>
        /// 反审2023.11.26
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="auditStatus"></param>
        /// <param name="auditReason"></param>
        /// <returns></returns>
        public virtual WebResponseContent AntiAudit(AntiData antiData)
        {
            if (antiData.Key == null)
                return Response.Error("未获取到参数!");

            Expression<Func<T, bool>> whereExpression = typeof(T).GetKeyName().CreateExpression<T>(antiData.IsFlow ? antiData.Key : antiData.Key.ToString().Split(",")[0], LinqExpressionType.Equal);
            T entity = repository.FindAsIQueryable(whereExpression).FirstOrDefault();
            if (entity == null)
            {
                return Response.Error($"未查到数据,或者数据已被删除,id:{antiData.Key}");
            }
            var auditProperty = GetAuditProperty();
            // var auditProperty = TProperties.Where(x => x.Name.ToLower() == "auditstatus").FirstOrDefault();
            if (auditProperty == null)
            {
                return Response.Error("表缺少审核状态字段：AuditStatus");
            }
            antiData.IsFlow = WorkFlowManager.Exists<T>(entity);
            if (!antiData.IsFlow)
            {
                int val = auditProperty.GetValue(entity).GetInt();
                if ((val == (int)AuditStatus.待审核 || val == (int)AuditStatus.草稿 || val == (int)AuditStatus.待提交))
                {
                    return Response.Error("只能选择已审核数据");
                }
            }
            Response.OK("反审成功".Translator());

            if (base.AntiAuditOnExecuting != null)
            {
                Response = AntiAuditOnExecuting(entity);
                if (CheckResponseResult()) return Response;
            }
            //审批流程返审
            if (antiData.IsFlow)
            {
                Response = WorkFlowManager.AntiAudit<T>(antiData, repository.BaseDbContext, entity, WorkFlowTableName);
                if (!Response.Status)
                {
                    return Response;
                }
                //RestartWorkFlowAudit(antiData.Key, $"[{UserContext.Current.UserTrueName}]反审了数据", FlowWriteState.重新开始);
            }
            else
            {
                auditProperty.SetValue(entity, (int)AuditStatus.待审核);
                repository.SqlSugarClient.Update(entity, new string[] { auditProperty.Name }, true);
                WorkFlowManager.AddAuditLog<T>(new object[] { antiData.Key }, (int)AuditStatus.待审核, "反审：" + (antiData.AuditReason ?? ""));
            }
            if (base.AntiAuditOnExecuted != null)
            {
                Response = AntiAuditOnExecuted(entity);
                if (CheckResponseResult()) return Response;
            }
            return Response;
        }


        public virtual (string, T, bool) ApiValidate(string bizContent, Expression<Func<T, object>> expression = null)
        {
            return ApiValidateInput<T>(bizContent, expression);
        }

        /// <summary>
        /// 对指定类与api的参数进行验证
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="bizContent"></param>
        /// <param name="input"></param>
        /// <param name="expression">对指属性验证</param>
        /// <returns>(string,TInput, bool) string:返回验证消息,TInput：bizContent序列化后的对象,bool:验证是否通过</returns>
        public virtual (string, TInput, bool) ApiValidateInput<TInput>(string bizContent, Expression<Func<TInput, object>> expression)
        {
            return ApiValidateInput(bizContent, expression, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="bizContent"></param>
        /// <param name="expression">对指属性验证格式如：x=>new { x.UserName,x.Value }</param>
        /// <param name="validateExpression">对指定的字段只做合法性判断比如长度是是否超长</param>
        /// <returns>(string,TInput, bool) string:返回验证消息,TInput：bizContent序列化后的对象,bool:验证是否通过</returns>
        public virtual (string, TInput, bool) ApiValidateInput<TInput>(string bizContent, Expression<Func<TInput, object>> expression, Expression<Func<TInput, object>> validateExpression)
        {
            try
            {
                TInput input = JsonConvert.DeserializeObject<TInput>(bizContent);
                if (!(input is System.Collections.IList))
                {
                    Response = input.ValidationEntity(expression, validateExpression);
                    return (Response.Message, input, Response.Status);
                }
                System.Collections.IList list = input as System.Collections.IList;
                for (int i = 0; i < list.Count; i++)
                {
                    Response = list[i].ValidationEntity(expression?.GetExpressionProperty(),
                        validateExpression?.GetExpressionProperty());
                    if (CheckResponseResult())
                        return (Response.Message, default(TInput), false);
                }
                return ("", input, true);
            }
            catch (Exception ex)
            {
                Response.Status = false;
                Response.Message = ApiMessage.ParameterError;
                Logger.Error(LoggerType.HandleError, bizContent, null, ex.Message);
            }
            return (Response.Message, default(TInput), Response.Status);
        }

        /// <summary>
        /// 将数据源映射到新的数据中,目前只支持List<TSource>映射到List<TResult>或TSource映射到TResult
        /// 目前只支持Dictionary或实体类型
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="resultExpression">只映射返回对象的指定字段</param>
        /// <param name="sourceExpression">只映射数据源对象的指定字段</param>
        /// 过滤条件表达式调用方式：List表达式x => new { x[0].MenuName, x[0].Menu_Id}，表示指定映射MenuName,Menu_Id字段
        ///  List<Sys_Menu> list = new List<Sys_Menu>();
        ///  list.MapToObject<List<Sys_Menu>, List<Sys_Menu>>(x => new { x[0].MenuName, x[0].Menu_Id}, null);
        ///  
        ///过滤条件表达式调用方式：实体表达式x => new { x.MenuName, x.Menu_Id}，表示指定映射MenuName,Menu_Id字段
        ///  Sys_Menu sysMenu = new Sys_Menu();
        ///  sysMenu.MapToObject<Sys_Menu, Sys_Menu>(x => new { x.MenuName, x.Menu_Id}, null);
        /// <returns></returns>
        public virtual TResult MapToEntity<TSource, TResult>(TSource source, Expression<Func<TResult, object>> resultExpression,
            Expression<Func<TSource, object>> sourceExpression = null) where TResult : class
        {
            return source.MapToObject<TSource, TResult>(resultExpression, sourceExpression);
        }

        /// <summary>
        /// 将一个实体的赋到另一个实体上,应用场景：
        /// 两个实体，a a1= new a();b b1= new b();  a1.P=b1.P; a1.Name=b1.Name;
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="result"></param>
        /// <param name="expression">指定对需要的字段赋值,格式x=>new {x.Name,x.P},返回的结果只会对Name与P赋值</param>
        public virtual void MapValueToEntity<TSource, TResult>(TSource source, TResult result, Expression<Func<TResult, object>> expression = null) where TResult : class
        {
            source.MapValueToEntity<TSource, TResult>(result, expression);
        }


        /// <summary>
        /// 2021.07.04增加code="-1"强制返回，具体使用见：后台开发文档->后台基础代码扩展实现
        /// </summary>
        /// <returns></returns>
        private bool CheckResponseResult()
        {
            return !Response.Status || Response.Code == "-1";
        }

        private PropertyInfo GetLogicDelProperty<TLg>()
        {

            return GetLogicDelProperty(typeof(TLg));
        }

        private PropertyInfo GetLogicDelProperty(Type type)
        {
            if (string.IsNullOrEmpty(AppSetting.LogicDelField))
            {
                return null;
            }
            return type.GetProperty(AppSetting.LogicDelField);
        }
    }

    public enum DelStatus
    {
        正常 = 0,
        已删除 = 1
    }
    public enum TableFlag
    {
        Add = 1,
        Update = 2,
        Del = 3
    }
    public class MultipleTableEntity
    {

        public Type Type { get; set; }
        public object List { get; set; }

        public TableFlag Flag { get; set; }

        public List<string> Fields { get; set; }

        public Type SubType { get; set; }

        public List<string> SubFields { get; set; }
    }
}
