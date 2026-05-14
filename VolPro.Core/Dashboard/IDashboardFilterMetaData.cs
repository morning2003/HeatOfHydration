using Dapper;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolPro.Core.Dashboard
{
    public interface IDashboardFilterMetaData
    {
        (string sql, List<SugarParameter> parameters) OnActionExecuting(string sql, List<SugarParameter> parameters, string dbService, bool isProc, DateTime? date1, DateTime? date2, string filterType);
    }
}
