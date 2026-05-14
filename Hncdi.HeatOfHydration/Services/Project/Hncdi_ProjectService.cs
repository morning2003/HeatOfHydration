/*
 *Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下Hncdi_ProjectService与IHncdi_ProjectService中编写
 */
using Hncdi.HeatOfHydration.IRepositories;
using Hncdi.HeatOfHydration.IServices;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace Hncdi.HeatOfHydration.Services
{
    public partial class Hncdi_ProjectService : ServiceBase<Hncdi_Project, IHncdi_ProjectRepository>
    , IHncdi_ProjectService, IDependency
    {
    public static IHncdi_ProjectService Instance
    {
      get { return AutofacContainerModule.GetService<IHncdi_ProjectService>(); } }
    }
 }
