using VolPro.Core.DBManager;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.SystemModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using VolPro.Core.DbSqlSugar;

namespace VolPro.Core.EFDbContext
{
    public class TestDbContext : BaseDbContext, IDependency
    {

        public TestDbContext() : base() {
            base.SqlSugarClient = DbManger.GetConnection(nameof(TestDbContext));
        }
    }
}
