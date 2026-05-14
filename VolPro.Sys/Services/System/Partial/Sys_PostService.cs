/*
 *所有关于Sys_Post类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*Sys_PostService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;
using System.Linq;
using VolPro.Core.Utilities;
using System.Linq.Expressions;
using VolPro.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using VolPro.Sys.IRepositories;
using VolPro.Core.ManageUser;
using System.Collections.Generic;
using VolPro.Core.UserManager;
using VolPro.Core.Configuration;
using VolPro.Core.Tenancy;
using SqlSugar;

namespace VolPro.Sys.Services
{
    public partial class Sys_PostService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISys_PostRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public Sys_PostService(
            ISys_PostRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            IsMultiTenancy = false;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override PageGridData<Sys_Post> GetPageData(PageDataOptions options)
        {
            FilterData();
            return base.GetPageData(options);
        }
        public override WebResponseContent Export(PageDataOptions pageData)
        {
            FilterData();
            return base.Export(pageData);
        }
        private void FilterData()
        {
            QueryRelativeExpression = (ISugarQueryable<Sys_Post> queryable) =>
            {
                if (AppSetting.UseDynamicShareDB)
                {
                    return queryable.Where(x => x.DbServiceId == UserContext.CurrentServiceId);
                }
                return queryable.FilterTenancy();
            };
        }

        WebResponseContent webResponse = new WebResponseContent();
        public override WebResponseContent Add(SaveModel saveDataModel)
        {
            AddOnExecuting = (Sys_Post post, object list) =>
            {
                post.DbServiceId = UserContext.CurrentServiceId;
                return webResponse.OK();
            };
            return base.Add(saveDataModel);
        }
        public override WebResponseContent Update(SaveModel saveModel)
        {
            UpdateOnExecuting = (Sys_Post post, object addList, object updateList, List<object> delKeys) =>
            {
                if (post.ParentId == post.PostId)
                {
                    return webResponse.Error("上级岗位不能选择自己");
                }
                if (_repository.Exists(x => x.PostId == post.ParentId && x.ParentId == post.PostId))
                {
                    return webResponse.Error("不能选择此上级岗位");
                }
                return webResponse.OK();
            };
            return base.Update(saveModel).Reload();
        }

    }
}
