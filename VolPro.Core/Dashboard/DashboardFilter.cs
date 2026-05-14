using Dapper;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.ManageUser;

namespace VolPro.Core.Dashboard
{
    /// <summary>
    /// 自定义设置工作台参数
    /// </summary>
    public class DashboardFilter : IDashboardFilterMetaData
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql">前端配置的sql或存储过程名称</param>
        /// <param name="dbService">选择的数据库</param>
        /// <param name="isProc">是否存储过程</param>
        /// <param name="date1">查询日期1</param>
        /// <param name="date2">查询日期1</param>
        /// <param name="filterType">过滤类型：今天、近7天...近一年等</param>
        /// <returns></returns>
        public (string sql, List<SugarParameter> parameters) OnActionExecuting(string sql, List<SugarParameter> parameters, string dbService, bool isProc, DateTime? date1, DateTime? date2, string filterType)
        {
            //根据不同的类型处理不同的值
            //if (isProc)
            //{
            //    if (sql == "存储过程名字")
            //    {
            //        sql = "";
            //    }
            //}

            ////手动在后台设置参数
            ////如：前端sql配置：select * from table where createId=@userId
            ////在这里就可以设置参数
            //parameters.Add("@userId", UserContext.Current.UserId);

            return (sql, parameters);
        }
    }
}
