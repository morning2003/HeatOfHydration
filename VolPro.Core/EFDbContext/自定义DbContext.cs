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
    public class 自定义DbContext : BaseDbContext, IDependency
    {
        public 自定义DbContext() : base()
        {
            base.SqlSugarClient = DbManger.GetConnection(nameof(自定义DbContext));
        }
    }
}
