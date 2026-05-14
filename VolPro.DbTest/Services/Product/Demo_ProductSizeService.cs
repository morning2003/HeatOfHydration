/*
 *Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下Demo_ProductSizeService与IDemo_ProductSizeService中编写
 */
using VolPro.DbTest.IRepositories;
using VolPro.DbTest.IServices;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace VolPro.DbTest.Services
{
    public partial class Demo_ProductSizeService : ServiceBase<Demo_ProductSize, IDemo_ProductSizeRepository>
    , IDemo_ProductSizeService, IDependency
    {
    public Demo_ProductSizeService(IDemo_ProductSizeRepository repository)
    : base(repository)
    {
    Init(repository);
    }
    public static IDemo_ProductSizeService Instance
    {
      get { return AutofacContainerModule.GetService<IDemo_ProductSizeService>(); } }
    }
 }
