using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using VolPro.Core.Configuration;
using VolPro.Core.Const;
using VolPro.Core.Dapper;
using VolPro.Core.EFDbContext;
using VolPro.Core.Enums;
using VolPro.Core.Extensions;
using MySqlConnector;
using VolPro.Core.ManageUser;
using VolPro.Entity.SystemModels;
using VolPro.Entity;
using Microsoft.Extensions.DependencyModel;
using System.Reflection;
using System.Runtime.Loader;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using VolPro.Core.DbSqlSugar;
using SqlSugar;
using VolPro.Core.CacheManager;

namespace VolPro.Core.DBManager
{
    public partial class DBServerProvider : DbManger
    {
        ////系统库
        private static readonly string DefaultConnName = nameof(SysDbContext);

        static DBServerProvider()
        {
        }

        public static void SetConnection(string key, string val)
        {
            DbRelativeCache.DbContextConnection[key] = val;

        }
        public static string GetConnectionString(string key)
        {
            return DbRelativeCache.DbContextConnection[key ?? DefaultConnName];
        }


        /// <summary>
        /// 获取系统库
        /// </summary>
        public static SysDbContext DbContext
        {
            get
            {
                SysDbContext dbContext = Utilities.HttpContext.Current.RequestServices.GetService(typeof(SysDbContext)) as SysDbContext;
                return dbContext;
            }
        }
        /// <summary>
        /// 根据实体model获取对应EF
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static BaseDbContext GetEntityDbContext<TEntity>()
        {
            string dbServer = typeof(TEntity).GetTypeCustomValue<EntityAttribute>(x => x.DBServer);

            return Utilities.HttpContext.Current.RequestServices.GetService(DbRelativeCache.GetDbContextType(dbServer)) as BaseDbContext;
        }

        /// <summary>
        /// 指定获取数据库
        /// </summary>
        /// <param name="dbService"></param>
        /// <returns></returns>
        public static string GetDbEntityName(string dbServer)
        {
            return DbRelativeCache.GetDbEntityType(dbServer).Name;
        }


        //public static ISqlSugarClient GetSqlSugarClient(string connectionKey = null)
        //{
        //    return DbManger.GetConnection(connectionKey);
        //}
        /// <summary>
        /// 根据dbcontext名称获取数据库链接
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetDbConnectionString(string key)
        {
            if (DbRelativeCache.DbContextConnection.TryGetValue(key, out string connString))
            {
                return connString;
            }
            throw new Exception($"未配置[{key}]的数据库连接");
        }

        public static string GetDbConnectionString(Type dbContext)
        {
            return GetDbConnectionString(dbContext.Name);
        }
        /// <summary>
        /// 获取系统库的字符串连接
        /// </summary>
        public static string SysConnectingString
        {
            get { return GetDbConnectionString(DefaultConnName); }
        }

        /// <summary>
        /// 获取业务库的字符串连接
        /// </summary>
        public static string ServiceConnectingString
        {
            get
            {
                //动态无限分库获取用户当前选择的数据库
                if (AppSetting.UseDynamicShareDB)
                {
                    return GetDbConnectionString(UserContext.CurrentServiceId.ToString());
                }
                return GetDbConnectionString(nameof(ServiceDbContext));
            }
        }
        /// <summary>
        /// 动态租户分库，获取指定数据库id的链接(2023.11.05)
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public static string GetServiceConnectingString(Guid serviceId)
        {
            return DbRelativeCache.DbContextConnection[serviceId.ToString()];
        }
    }
}
