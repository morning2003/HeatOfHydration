using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.Configuration;
using VolPro.Core.Const;
using VolPro.Core.DBManager;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.EFDbContext;
using VolPro.Core.Utilities;
using VolPro.Entity.DomainModels;

namespace VolPro.Core.CacheManager
{
    public static class DbCache
    {
        private static List<Sys_DbService> DbServices = null;
        private static object _lock_sbcnew = new object();


        public static void Init()
        {
            DbServices = DbManger.SysDbContext.Set<Sys_DbService>().Select(s => new
            {
                s.Pwd,
                s.DbIpAddress,
                s.DatabaseName,
                s.DbServiceId,
                s.GroupId,
                s.UserId,
                s.Remark,
                s.PhoneNo,
                s.DbServiceName
            })
                //.Where(x => x.DatabaseName != null && x.DbIpAddress != null && x.UserId != null && x.Pwd != null)
                .ToList()
                .Select(s => new Sys_DbService()
                {
                    Pwd = s.Pwd,
                    DbIpAddress = s.DbIpAddress,
                    DatabaseName = s.DatabaseName,
                    DbServiceName = s.DbServiceName,
                    DbServiceId = s.DbServiceId,
                    GroupId= s.GroupId,
                    UserId = s.UserId,
                    Remark = s.Remark,
                    PhoneNo = s.PhoneNo
                }).ToList();
            InitConnection();


        }
        public static List<Sys_DbService> GetList()
        {
            return DbServices;
        }

        public static WebResponseContent Reload(WebResponseContent webResponse)
        {
            if (webResponse.Status)
            {
                Init();
            }
            return webResponse;
        }

        public static void InitConnection()
        {
            foreach (var item in DbServices)
            {
                InitConnection(item);
            }
        }

        public static string InitConnection(Sys_DbService item, string databaseName = null)
        {
            string connectionString = GetConnectionString(item, databaseName);

            if (databaseName == null)
            {
                DBServerProvider.SetConnection(item.DbServiceId.ToString(), connectionString);
            }
            return connectionString;
        }

        public static string GetConnectionString(Sys_DbService item, string databaseName = null)
        {
            string connectionString = null;
            switch (DBType.Name)
            {
                //mysql如果端口不是3306，这里也需要修改
                case "MySql":
                    connectionString = @$" Data Source={item.DbIpAddress};Database={databaseName ?? item.DatabaseName};AllowLoadLocalInfile=true;User ID={item.UserId};Password={item.Pwd};allowPublicKeyRetrieval=true;pooling=true;CharSet=utf8;port=3306;sslmode=none;";
                    break;
                case "PgSql":
                    connectionString = $"Host={item.DbIpAddress};Port=5432;User id={item.UserId};password={item.Pwd};Database={databaseName ?? item.DatabaseName};";

                    break;
                case "MsSql":
                    connectionString = @$"Data Source={item.DbIpAddress};Initial Catalog={databaseName ?? item.DatabaseName};Persist Security Info=True;User ID={item.UserId};Password={item.Pwd};Connect Timeout=500;Max Pool Size = 512;TrustServerCertificate=True;";

                    break;
                case "DM":
                    // 老版本 ：PORT=5236;DATABASE=DAMENG;HOST=localhost;PASSWORD=SYSDBA;USER ID=SYSDBA
                    //新版本： Server=localhost; User Id=SYSDBA; PWD=SYSDBA;DATABASE=新DB
                    connectionString = $" Server={item.DbIpAddress}; User Id={item.UserId}; PWD={item.Pwd};DATABASE={databaseName ?? item.DatabaseName}";
                    break;
                case "Oracle":
                    Console.WriteLine($"未实现数据库：{DBType.Name}");
                    break;
            }
            return connectionString;
        }


        public static Sys_DbService GetDbInfo(Guid dbServiceId)
        {
            return DbServices.Where(x => x.DbServiceId == dbServiceId).FirstOrDefault();
        }

        public static IEnumerable<Sys_DbService> GetDbInfo(Func<Sys_DbService, bool> where)
        {
            return DbServices.Where(where);
        }


    }
}
