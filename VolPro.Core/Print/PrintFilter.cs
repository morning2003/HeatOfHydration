using Microsoft.EntityFrameworkCore;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.EFDbContext;
using VolPro.Entity.DomainModels;

namespace VolPro.Core.Print
{
    public  class PrintFilter
    {
        public PrintFilter()
        { 
        
        }
        /// <summary>
        /// 主表查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public virtual ISugarQueryable<T> Query<T>(ISugarQueryable<T> query, PrintQuery parms) where T : class
        {
             return query;
        }
        /// <summary>
        /// 明细表查询
        /// </summary>
        /// <typeparam name="Detail"></typeparam>
        /// <param name="query"></param>
        /// <param name="parms"></param>
        /// <returns></returns>

        public virtual ISugarQueryable<Detail> QueryDetail<Detail>(ISugarQueryable<Detail> query, PrintQuery parms) where Detail : class
        {
            return query;
        }

        /// <summary>
        /// 查询结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Detail"></typeparam>
        /// <param name="result"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public virtual List<Dictionary<string, object>> QueryResult<T>(
            List<Dictionary<string, object>> result,
            PrintQuery parms,
            BaseDbContext dbContext)
        {
            return result;
        }
    }
}
