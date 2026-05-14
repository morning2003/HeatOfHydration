using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.Configuration;
using VolPro.Core.Extensions;
using VolPro.Core.ManageUser;
using VolPro.Entity.DomainModels;
using static Dapper.SqlMapper;

namespace VolPro.Core.Tenancy
{

    public static class TenancyDefault
    {
        /// <summary>
        /// 租户不分库时设置表的租户字段值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static T SetTenancyValue<T>(this T entity) where T : class
        {
            PropertyInfo property = GetTenancyProperty<T>();
            if (property != null)
            {
                property.SetValue(entity, UserContext.CurrentServiceId.ToString());
            }
            return entity;
        }

        public static List<T> SetTenancyValue<T>(this List<T> list) where T : class
        {
            PropertyInfo property = GetTenancyProperty<T>();
            if (property != null)
            {
                var tenancyId = UserContext.CurrentServiceId.ToString();
                foreach (var entity in list)
                {
                    property.SetValue(entity, tenancyId);
                }
            }
            return list;
        }
        /// <summary>
        /// 租户不分库时统一过滤当前租户表的数据
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static ISugarQueryable<T> FilterTenancy<T>(this ISugarQueryable<T> query) where T : class
        {
            PropertyInfo property = GetTenancyProperty<T>();
            if (property != null)
            {
                var where = property.Name.CreateExpression<T>(UserContext.CurrentServiceId.ToString(), Enums.LinqExpressionType.Equal);
                return query.Where(where);
            }
            return query;
        }

        private static PropertyInfo GetTenancyProperty<T>()
        {
            if (AppSetting.TenancyField == null)
            {
                return null;
            }
            return typeof(T).GetProperty(AppSetting.TenancyField);
        }
    }
}
