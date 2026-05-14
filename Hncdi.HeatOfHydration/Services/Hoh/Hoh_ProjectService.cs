/*
 *Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下Hoh_ProjectService与IHoh_ProjectService中编写
 */
using Hncdi.HeatOfHydration.IRepositories;
using Hncdi.HeatOfHydration.IServices;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Core.Utilities;
using VolPro.Entity.DomainModels;

namespace Hncdi.HeatOfHydration.Services
{
    public partial class Hoh_ProjectService : ServiceBase<Hoh_Project, IHoh_ProjectRepository>
    , IHoh_ProjectService, IDependency
    {
    public static IHoh_ProjectService Instance
    {
      get { return AutofacContainerModule.GetService<IHoh_ProjectService>(); } }
    }
 }
