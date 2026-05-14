/*
*所有关于Sys_Dashboard类的业务代码接口应在此处编写
*/
using VolPro.Core.BaseProvider;
using VolPro.Entity.DomainModels;
using VolPro.Core.Utilities;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VolPro.Sys.IServices
{
    public partial interface ISys_DashboardService
    {
        /// <summary>
        /// 编译、预览、查看获取全部配置
        /// </summary>
        /// <param name="id"></param>
        /// <param name="view"></param>
        /// <returns></returns>

        Task<object> GetLayoutData(Guid id, bool view);
        /// <summary>
        ///  获取每项sql数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        Task<object> GetItemData(Guid id, string itemId, DateTime? date1, DateTime? date2, string filterType);

        /// <summary>
        /// 编译执行sql
        /// </summary>
        /// <param name="id"></param>
        /// <param name="view"></param>
        /// <returns></returns>

        Task<object> ExecSql(Dictionary<string, string> dic);

    }
}
