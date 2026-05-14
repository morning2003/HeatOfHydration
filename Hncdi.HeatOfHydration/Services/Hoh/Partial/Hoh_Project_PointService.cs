/*
 *所有关于Hoh_Project_Point类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*Hoh_Project_PointService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
    public partial class Hoh_Project_PointService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHoh_Project_PointRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public Hoh_Project_PointService(
            IHoh_Project_PointRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }


    }
}
