/*
 *Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下Demo_CatalogService与IDemo_CatalogService中编写
 */
using VolPro.DbTest.IRepositories;
using VolPro.DbTest.IServices;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace VolPro.DbTest.Services
{
    public partial class Demo_CatalogService : ServiceBase<Demo_Catalog, IDemo_CatalogRepository>
    , IDemo_CatalogService, IDependency 
    { 
    public Demo_CatalogService(IDemo_CatalogRepository repository)
    : base(repository)
    {
    Init(repository);
    }
    public static IDemo_CatalogService Instance
    {
      get { return AutofacContainerModule.GetService<IDemo_CatalogService>(); } }
    }
 }
