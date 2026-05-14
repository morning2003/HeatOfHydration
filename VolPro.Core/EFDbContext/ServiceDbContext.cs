using VolPro.Core.DBManager;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.SystemModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using SqlSugar;
using VolPro.Core.DbSqlSugar;

namespace VolPro.Core.EFDbContext
{
    public class ServiceDbContext : BaseDbContext, IDependency
    {
        private string dbServiceId { get; set; }

        public ServiceDbContext() : base() 
        {
            this.dbServiceId = dbServiceId;
            base.SqlSugarClient = DbManger.ServiceDb;
        }

     
        public ServiceDbContext(string dbServiceId) : base()
        {
            this.dbServiceId = dbServiceId;
            base.SqlSugarClient = DbManger.ServiceDb;
        }
    }
}
