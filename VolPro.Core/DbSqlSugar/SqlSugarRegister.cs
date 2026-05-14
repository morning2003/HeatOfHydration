using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.Configuration;
using VolPro.Core.Const;
using VolPro.Core.DBManager;
using VolPro.Core.EFDbContext;
using VolPro.Core.Enums;
using VolPro.Core.Extensions;

namespace VolPro.Core.DbSqlSugar
{
    public static class SqlSugarRegister
    {

        /// <summary>
        ///系统库链接
        /// </summary>
        /// <returns></returns>
        public static ConnectionConfig GetSysConnectionConfig()
        {
            var dbType = DbManger.GetDbType();

            return new ConnectionConfig()
            {
                DbType = dbType,// SqlSugar.DbType.SqlServer, 
                ConnectionString = DBServerProvider.SysConnectingString,
                IsAutoCloseConnection = true,
                ConfigId = "default",
                MoreSettings = new ConnMoreSettings()
                {
                    PgSqlIsAutoToLower = false,
                    IsAutoToUpper = false,
                    // DatabaseModel = DbType.PostgreSQL 
                },
                ConfigureExternalServices = GetConfigureExternalServices()
            };
        }



        /// <summary>
        ///  模板空库(租户动态分才使用)
        /// </summary>
        /// <returns></returns>
        private static ConnectionConfig GetEmptyConnectionConfig()
        {
            //Console.WriteLine(AppSetting.GetSection("Connection")["EmptyDbContext"]);
            var dbType = DbManger.GetDbType();
            //模板空库(租户动态分才使用)
            return new ConnectionConfig()
            {
                DbType = dbType,// SqlSugar.DbType.SqlServer,
                ConnectionString = AppSetting.GetSection("Connection")["EmptyDbContext"],
                IsAutoCloseConnection = true,
                ConfigId = "EmptyDbContext",
                MoreSettings = new ConnMoreSettings()
                {
                    PgSqlIsAutoToLower = false,
                    IsAutoToUpper = false
                },
                ConfigureExternalServices = GetConfigureExternalServices()
            };
        }

        public static IServiceCollection UseSqlSugar(this IServiceCollection services)
        {
            StaticConfig.DynamicExpressionParserType = typeof(DynamicExpressionParser);
            services.AddHttpContextAccessor();

            var dbType = DbManger.GetDbType();  

            //缓存所有配置文件的中的数据库链接
            var configs = DbRelativeCache.DbContextConnection
                .Where(x => x.Key.EndsWith("DbContext") || x.Key == "default").Select(s => new ConnectionConfig()
                {
                    //2024.01.22增加分库使用不同类型的数据库
                    DbType = SqlSugarDbType.GetType(s.Key),// SqlSugar.DbType.SqlServer,
                    ConnectionString = s.Value,
                    IsAutoCloseConnection = true,
                    ConfigId = s.Key,
                    MoreSettings = new ConnMoreSettings()
                    {
                        PgSqlIsAutoToLower = false,
                        IsAutoToUpper = false
                    },
                    //https://www.donet5.com/Home/Doc?typeId=1182
                    ConfigureExternalServices = GetConfigureExternalServices()
                }).ToList();

            configs.Add(GetEmptyConnectionConfig());
            services.AddSingleton<ISqlSugarClient>(s =>
            {
                var sysConfig = GetSysConnectionConfig();
                SqlSugarScope sqlSugar = new SqlSugarScope(
                configs,
                db =>
               {
                   //业务库日志
                   foreach (var item in configs.Where(x => x.ConfigId?.ToString() != "SysDbContext"))
                   {
                       string id = item.ConfigId.ToString();
                       if (db.GetConnection(id) != null)
                       {
                           db.GetConnection(id).Aop.OnLogExecuting = (sql, pars) =>
                           {
                               Console.WriteLine(sql);
                           };
                       }
                   };
                   //单例参数配置，所有上下文生效
                   db.Aop.OnLogExecuting = (sql, pars) =>
                   {
                       Console.WriteLine(sql);
                   };

               });
                return sqlSugar;
            });
            return services;
        }

        /// <summary>
        /// 设置字段全大写
        /// </summary>
        /// <returns></returns>
        private static ConfigureExternalServices GetConfigureExternalServices()
        {
            //https://www.donet5.com/Home/Doc?typeId=1182
            return new ConfigureExternalServices()
            {
                EntityService = (property, column) =>
                {
                    if (DBType.Name == "DM")
                    {
                       // var attributes = property.GetCustomAttributes(true);//get all attributes 
                        column.DbColumnName = property.Name.ToUpper();
                    }
                }
            };
        }

    }
}
