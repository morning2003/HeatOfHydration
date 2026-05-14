/*
 *所有关于Sys_ReportOptions类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*Sys_ReportOptionsService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using System.Collections.Generic;

namespace VolPro.Sys.Services
{
    public partial class Sys_ReportOptionsService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISys_ReportOptionsRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public Sys_ReportOptionsService(
            ISys_ReportOptionsRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override WebResponseContent Add(SaveModel saveDataModel)
        {
            saveDataModel.MainData["ReportCode"] = new IdWorker().NextId().ToString();
            return base.Add(saveDataModel);
        }

        WebResponseContent webResponse = new WebResponseContent();
        public override WebResponseContent Upload(List<IFormFile> files)
        {
            if (!files.Any(x=>x.FileName.ToLower().EndsWith(".grf")))
            {
                return webResponse.Error("只能上传grf格式文件");
            }
            IsRoot = false;
            UploadFolder = "ReportTemplate/";
            return base.Upload(files);
        }
    }
}
