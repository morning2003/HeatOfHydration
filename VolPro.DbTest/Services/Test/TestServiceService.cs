/*
 *Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下TestServiceService与ITestServiceService中编写
 */
using VolPro.DbTest.IRepositories;
using VolPro.DbTest.IServices;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;
using Microsoft.AspNetCore.Http;

namespace VolPro.DbTest.Services
{
    public partial class TestServiceService : ServiceBase<TestService, ITestServiceRepository>
    , ITestServiceService, IDependency
    {
        public TestServiceService(ITestServiceRepository repository)
        : base(repository)
        {
            Init(repository);
        }
        public static ITestServiceService Instance
        {
            get { return AutofacContainerModule.GetService<ITestServiceService>(); }
        }
    }
}
