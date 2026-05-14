/*
 *Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下Demo_OrderListService与IDemo_OrderListService中编写
 */
using VolPro.DbTest.IRepositories;
using VolPro.DbTest.IServices;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace VolPro.DbTest.Services
{
    public partial class Demo_OrderListService : ServiceBase<Demo_OrderList, IDemo_OrderListRepository>
    , IDemo_OrderListService, IDependency
    {
    public Demo_OrderListService(IDemo_OrderListRepository repository)
    : base(repository)
    {
    Init(repository);
    }
    public static IDemo_OrderListService Instance
    {
      get { return AutofacContainerModule.GetService<IDemo_OrderListService>(); } }
    }
 }
