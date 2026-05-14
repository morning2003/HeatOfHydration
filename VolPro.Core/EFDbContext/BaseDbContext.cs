using VolPro.Core.Configuration;
using VolPro.Core.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using VolPro.Core.Const;
using VolPro.Core.Enums;
using VolPro.Core.BaseProvider;
using VolPro.Core.DbSqlSugar;
using SqlSugar;

namespace VolPro.Core.EFDbContext
{
    public abstract class BaseDbContext : DbContext
    {
    

        public ISqlSugarClient SqlSugarClient { get; set; }

        public bool QueryTracking
        {
            set
            {
            }
        }
        public BaseDbContext():base() { }

        public ISugarQueryable<TEntity> Set<TEntity>(bool filterDeleted=false) where TEntity : class
        {
            return SqlSugarClient.Set<TEntity>(filterDeleted);
        }

        public int SaveChanges()
        {
            return SqlSugarClient.SaveQueues();
        }

   


        protected void UseDbType(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            //if (DBType.Name == DbCurrentType.MsSql.ToString())
            //{
            //    if (AppSetting.UseSqlserver2008)
            //    {
            //        optionsBuilder.ReplaceService<IQueryTranslationPostprocessorFactory, SqlServer2008QueryTranslationPostprocessorFactory>();
            //    }
            //    optionsBuilder.UseSqlServer(connectionString);
            //}
            //else if (DBType.Name == DbCurrentType.MySql.ToString())
            //{
            //    optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 11)));
            //}
            //else if (DBType.Name == DbCurrentType.PgSql.ToString())
            //{
            //    optionsBuilder.UseNpgsql(connectionString);
            //}
            //else if (DBType.Name == DbCurrentType.Oracle.ToString())
            //{
            //    optionsBuilder.UseOracle(connectionString, b => b.UseOracleSQLCompatibility("11"));
            //}
            //else
            //{
            //    throw new Exception("数据库未实现");
            //    //  optionsBuilder.UseSqlServer(connectionString);
            //}

        }

        //protected void OnModelCreating(ModelBuilder modelBuilder, Type type)
        //{
        //    try
        //    {
        //        //获取所有类库
        //        var compilationLibrary = DependencyContext
        //            .Default
        //            .CompileLibraries
        //            .Where(x => !x.Serviceable && x.Type != "package" && x.Type == "project");
        //        foreach (var _compilation in compilationLibrary)
        //        {
        //            //加载指定类  
        //            AssemblyLoadContext.Default
        //            .LoadFromAssemblyName(new AssemblyName(_compilation.Name))
        //            .GetTypes().Where(x => x.GetTypeInfo().BaseType != null
        //            && x.BaseType == (type)).ToList()
        //            .ForEach(t => { modelBuilder.Entity(t); });
        //        }

        //        //Oracle数据库指定表名与列名全部大写
        //        if (DBType.Name == DbCurrentType.Oracle.ToString())
        //        {
        //            foreach (var entity in modelBuilder.Model.GetEntityTypes())
        //            {
        //                string tableName = entity.GetTableName().ToUpper();
        //                if (tableName.StartsWith("SYS_") || tableName.StartsWith("DEMO_"))
        //                {
        //                    entity.SetTableName(entity.GetTableName().ToUpper());
        //                    foreach (var property in entity.GetProperties())
        //                    {
        //                        property.SetColumnName(property.Name.ToUpper());
        //                    }
        //                }
        //            }
        //        }

        //        //重置系统表名小写,如果是mysql数据库，创建的表名都是小写的，请取消此注释
        //        //foreach (var entity in modelBuilder.Model.GetEntityTypes())
        //        //{
        //        //    
        //        //    if (entity.GetTableName().StartsWith("Sys_"))
        //        //    {
        //        //        Console.WriteLine(entity.GetTableName());
        //        //        entity.SetTableName(entity.GetTableName().ToLower());
        //        //    } 
        //        //    //// 重置所有列名
        //        //    //foreach (var property in entity.GetProperties())
        //        //    //{
        //        //    //    //StoreObjectIdentifier
        //        //    //    property.SetColumnName(property.Name);
        //        //    //}
        //        //}

        //        //插件式开发
        //        //try
        //        //{
        //        //    string rootPath = (AppSetting.CurrentPath + "\\plugs").ReplacePath();
        //        //    foreach (var item in Directory.GetFiles(rootPath).Where(x => x.EndsWith(".dll")))
        //        //    {
        //        //        string path = ($"{item}").ReplacePath();
        //        //        Assembly.LoadFile(path).GetTypes().Where(x => x.GetTypeInfo().BaseType != null
        //        //        && x.BaseType == (type)).ToList()
        //        //     .ForEach(t => {
        //        //         Console.Write(t.Name);
        //        //         modelBuilder.Entity(t);

        //        //     });
        //        //    }
        //        //}
        //        //catch (Exception ex)
        //        //{
        //        //    Console.WriteLine($"EF解析类库异常：{ex.Message + ex.StackTrace}");
        //        //}

        //        base.OnModelCreating(modelBuilder);
        //    }
        //    catch (Exception ex)
        //    {
        //        string mapPath = ($"Log/").MapPath();
        //        Utilities.FileHelper.WriteFile(mapPath, $"syslog_{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt", ex.Message + ex.StackTrace + ex.Source);
        //    }

       // }

    }
}
