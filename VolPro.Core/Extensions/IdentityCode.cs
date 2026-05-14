using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.BaseProvider;
using VolPro.Core.Configuration;
using VolPro.Core.DBManager;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.ManageUser;
using VolPro.Core.Tenancy;
using VolPro.Entity.DomainModels;

namespace VolPro.Core.Extensions
{

    public static class IdentityCode
    {

        private static List<Sys_CodeRule> _codeRules = null;
        private static object ruleObject = new object();

        public static void Init()
        {
            _codeRules = null;
        }

        public static List<Sys_CodeRule> CodeRules
        {
            get
            {
                if (_codeRules == null)
                {
                    lock (ruleObject)
                    {
                        if (_codeRules == null)
                        {
                            _codeRules = DBServerProvider.DbContext.Set<Sys_CodeRule>().ToList();//.FilterTenancy()
                        }
                    }
                }
                return _codeRules;
            }
        }
        /// <summary>
        /// 生成单据号(先在[单据编码]菜单维护规则)
        /// 使用方式：var order = new DemoOrder();  
        ///              order.CreateCode();
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static T CreateCode<T>(this T entity) where T : class
        {
            CreateCode(new List<T>() { entity });
            return entity;
        }
        /// <summary>
        /// 批量生成单据号(先在[单据编码]菜单维护规则)
        /// 使用方式：var list =new List<DemoOrder>(){};  
        ///              list.CreateCode();
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static List<T> CreateCode<T>(this List<T> entity) where T : class
        {
            var query = CodeRules.Where(x => x.TableName == typeof(T).Name);
            //租户分库
            if (AppSetting.UseDynamicShareDB)
            {
                query = query.Where(x => x.DbServiceId == UserContext.CurrentServiceId);
            }
            else
            {
                if (AppSetting.TenancyField != null)
                {
                    query = query.Where(x => x.TenancyId == UserContext.CurrentServiceId.ToString());
                }
            }
            var rule = query.OrderByDescending(x => x.CreateDate).FirstOrDefault();
            if (rule == null)
            {
                return entity;
            }
            Create<T>(entity,
                 rule.Field.GetExpression<T>(),
                 rule.PrefixCode,
                 rule.OrderFiled.GetExpression<T>(),
                 filter: null,
                 startingDay: rule.RuleIncremental == "day",//这里待完,只处理了是否每天每生成与一直接自增
                 dateFormat: rule.RuleType,
                 rule.ValueLen,
                 rule.ConcatenationSymbol);
            return entity;
        }

        /// <summary>
        /// 创建自增单据号
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <param name="codeField">要设置单据号的字段</param>
        /// <param name="preCode">单据号前缀,如：{TC}{2023}{0001}</param>
        /// <param name="dateFieldExpression">排序字段，每天都从第1个号码开始</param>
        /// <param name="filter">过滤条件</param>
        /// <param name="startingDay">是否每天都从第1个号码开始</param>
        /// <param name="dateFormat">是否生成日期流水号</param>
        /// <param name="len">数字长度</param>
        /// 使用示例：
        ///   Sys_User user=  new Sys_User();
        ///   user.Create(x => x.UserName, "U", x => x.CreateDate);
        /// <returns></returns>
        public static string Create<T>(this T entity,
            Expression<Func<T, object>> codeField,
            string preCode = "Code",
            Expression<Func<T, object>> dateFieldExpression = null,
            Expression<Func<T, bool>> filter = null,
            bool startingDay = true,
            string dateFormat = "yyyyMMdd",
            int len = 4,
            string concatenationSymbol = null
            ) where T : class
        {
            return new List<T>() { entity }.Create(codeField, preCode, dateFieldExpression, filter, startingDay, dateFormat, len, concatenationSymbol);
        }

        public static string Create<T>(this List<T> list,
         Expression<Func<T, object>> codeField,
         string preCode = "Code",
         Expression<Func<T, object>> dateFieldExpression = null,
         Expression<Func<T, bool>> filter = null,
         bool startingDay = true,
         string dateFormat = "yyyyMMdd",
         int len = 4,
          string concatenationSymbol = null
         ) where T : class
        {

            if (concatenationSymbol == null)
            {
                concatenationSymbol = "";
            }
            string dateField;

            if (dateFieldExpression == null)
            {
                dateField = AppSetting.CreateMember.DateField;
            }
            else
            {
                dateField = dateFieldExpression.GetExpressionPropertyFirst();
            }
            var field = codeField.GetExpressionPropertyFirst();

            DateTime dateNow = (DateTime)DateTime.Now.ToString("yyyy-MM-dd").GetDateTime();
            var condition = dateField.CreateExpression<T>(dateNow, Enums.LinqExpressionType.ThanOrEqual);

            Expression<Func<T, bool>> conditionStartWdth = null;
            //增加指定开头单据号的查询，避免其他非单据号影响
            if (!string.IsNullOrEmpty(preCode))
            {
                conditionStartWdth = field.CreateExpression<T>(preCode, Enums.LinqExpressionType.LikeStart);
            }

            var select = field.GetExpression<T, string>();
            string orderNo = DBServerProvider.GetEntityDbContext<T>().Set<T>()
                .WhereIF(filter == null && startingDay, condition)
                 .WhereIF(conditionStartWdth != null, conditionStartWdth)
                .OrderByDescending(codeField)
                .Select(select)
                .FirstOrDefault()
                ?.ToString();
            string rule;
            if (dateFormat != null)
            {
                rule = $"{preCode}{concatenationSymbol}{DateTime.Now.ToString(dateFormat)}";
            }
            else
            {
                rule = preCode;
            }

            int number = 0;
            if (!string.IsNullOrEmpty(orderNo))
            {
                number = orderNo.Substring(orderNo.Length - len).GetInt();
            }

            var property = typeof(T).GetProperty(field);
            string code = null;
            foreach (var entity in list)
            {
                number++;
                code = $"{rule}{concatenationSymbol}{(number).ToString("D" + len)}";
                property.SetValue(entity, code);
            }
            return code;
        }
    }
}
