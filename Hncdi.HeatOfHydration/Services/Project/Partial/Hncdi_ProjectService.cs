/*
 *所有关于Hncdi_Project类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*Hncdi_ProjectService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using Hncdi.HeatOfHydration.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Linq.Expressions;
using VolPro.Core.BaseProvider;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.Extensions;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Core.Utilities;
using VolPro.Entity.DomainModels;

namespace Hncdi.HeatOfHydration.Services
{
    public partial class Hncdi_ProjectService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHncdi_ProjectRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public Hncdi_ProjectService(
            IHncdi_ProjectRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }
        public override WebResponseContent Del(object[] keys, bool delList = true)
        {
            if (keys.Length > 1)
            {
                return WebResponseContent.Instance.Error("每次仅可删除一条数据");
            }
            WebResponseContent webResponse = new WebResponseContent();
            var ids = keys.Select(s => s.ToString().GetLong()).ToList()[0];
            bool b = repository.DbContext.Set<Hoh_Project>().Where(x => x.Project_id == ids).ToList().Count > 0;
            if (b)
            {
                return WebResponseContent.Instance.Error("该项目下存在监测部位，不能删除");
            }
            return base.Del(keys, delList);
        }
    }
}
