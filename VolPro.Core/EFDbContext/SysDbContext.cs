using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Core.DBManager;
using VolPro.Entity.SystemModels;
using System.Reflection.Emit;
using VolPro.Core.DbSqlSugar;

namespace VolPro.Core.EFDbContext
{
    public class SysDbContext : BaseDbContext, IDependency
    {
        public SysDbContext() : base() {
            base.SqlSugarClient = DbManger.SysDbContext;
        }
    }
}
