using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using VolPro.Core.Configuration;
using VolPro.Core.EFDbContext;
using VolPro.Core.ManageUser;
using VolPro.Entity.SystemModels;

namespace VolPro.Core.DBManager
{
    public static class DbRelativeCache
    {
        private static Dictionary<string, Type> DbContextTypes = new Dictionary<string, Type>();
        private static Dictionary<string, Type> DbEntityTypes = new Dictionary<string, Type>();
        private static Dictionary<string, string> DbTypes = new Dictionary<string, string>();

        /// <summary>
        /// 所有数据库链接字符串
        /// </summary>
        private static Dictionary<string, string> DbConnection = new Dictionary<string, string>();

        public static Dictionary<string, string> DbContextConnection
        {
            get { return DbConnection; }
        }

        static DbRelativeCache()
        {
            InitDbContextType();
            InitDbEntityType();
        }
        /// <summary>
        /// 缓存分库DbContext
        /// </summary>
        public static void InitDbContextType()
        {
            var compilationLibrary = DependencyContext
                 .Default
                 .RuntimeLibraries
                 .Where(x => x.Name.EndsWith(".Core") && !x.Serviceable && x.Type != "package" && x.Type == "project");
            foreach (var _compilation in compilationLibrary)
            {
                //加载指定类
                foreach (var item in AssemblyLoadContext.Default
                .LoadFromAssemblyName(new AssemblyName(_compilation.Name))
                .GetTypes().Where(x => x.GetTypeInfo().BaseType != null
                && x.BaseType == (typeof(BaseDbContext))))
                {
                    DbContextTypes[item.Name] = item;
                    //获取数据库链接类型,在appsettings.json中Connection属性添加xxxDbType，前缀与数据库链接一样
                    //ServiceDbContext:"数据库链接字符"=>ServiceDbType:"MsSql";数据库链接类型

                    string typeName = item.Name.Replace("DbContext", "").Replace("Entity", "") + "DbType";
                    string dbType = AppSetting.GetSection("Connection")[typeName];
                    if (!string.IsNullOrEmpty(dbType))
                    {
                        DbTypes.TryAdd(item.Name, dbType);
                    }
                    //缓存数据库链接
                    string connectionString = AppSetting.GetSection("Connection")[item.Name];
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        DbConnection.TryAdd(item.Name, connectionString); 
                    }
                }
            }
            //缓存系统数据库链接
            DbConnection[nameof(SysDbContext)]= AppSetting.GetSection("Connection")["DbConnectionString"];
        }
        /// <summary>
        /// 缓存分库model基类
        /// </summary>
        public static void InitDbEntityType()
        {
            var compilationLibrary = DependencyContext
                 .Default
                 .RuntimeLibraries
                 .Where(x => x.Name.EndsWith(".Entity") && !x.Serviceable && x.Type != "package" && x.Type == "project");
            foreach (var _compilation in compilationLibrary)
            {
                //加载指定类
                foreach (var item in AssemblyLoadContext.Default
                .LoadFromAssemblyName(new AssemblyName(_compilation.Name))
                .GetTypes().Where(x => x.GetTypeInfo().BaseType != null
                && x.BaseType == (typeof(BaseEntity))))
                {
                    DbEntityTypes[item.Name] = item;
                }
            }
        }
        /// <summary>
        /// 获取数据库的链接类型。如数据库是mysql还是pgsql类型
        /// </summary>
        /// <param name="dbService"></param>
        /// <returns></returns>
        public static string GetDbType(string dbService)
        {
            if (string.IsNullOrEmpty(dbService))
            {
                return null;
            }
            DbTypes.TryGetValue(dbService, out string value);
            return value;
        }
        /// <summary>
        /// 根据分库名称获取dbcontext
        /// </summary>
        /// <param name="dbService"></param>
        /// <returns></returns>
        public static Type GetDbContextType(string dbService)
        {
            return DbContextTypes[dbService];
        }

        /// <summary>
        /// 根据分库名称获取分库model基类
        /// </summary>
        /// <param name="dbService"></param>
        /// <returns></returns>
        public static Type GetDbEntityType(string dbService)
        {
            Type dbContextType = DbContextTypes[dbService];
            string name = dbContextType.Name.Replace("DbContext", "");
            return DbEntityTypes[$"{name}Entity"];
        }

        /// <summary>
        /// 根据dbtype获取数据库链接
        /// </summary>
        /// <param name="dbContextType"></param>
        /// <returns></returns>
        public static string GetDbConnectionString(Type dbContextType)
        {
            return GetDbConnectionString(dbContextType.GetType().Name);
        }
        /// <summary>
        /// 根据dbtype获取数据库链接
        /// </summary>
        /// <param name="dbContextType"></param>
        /// <returns></returns>
        public static string GetDbConnectionString(string dbContextType)
        {
            if (dbContextType == null)
            {
                return null;
            }
            DbConnection.TryGetValue(dbContextType, out string value);
            if (dbContextType == nameof(ServiceDbContext))
            {
                if (AppSetting.UseDynamicShareDB)
                {
                    return DBServerProvider.GetDbConnectionString(UserContext.CurrentServiceId.ToString());
                }
            }
            return value;
        }
    }
}
