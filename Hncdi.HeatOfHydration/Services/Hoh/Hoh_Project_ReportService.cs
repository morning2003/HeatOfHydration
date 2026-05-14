/*
 *Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下Hoh_Project_ReportService与IHoh_Project_ReportService中编写
 */
using Hncdi.HeatOfHydration.IRepositories;
using Hncdi.HeatOfHydration.IServices;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace Hncdi.HeatOfHydration.Services
{
    public partial class Hoh_Project_ReportService : ServiceBase<Hoh_Project_Report, IHoh_Project_ReportRepository>
    , IHoh_Project_ReportService, IDependency
    {
    public static IHoh_Project_ReportService Instance
    {
      get { return AutofacContainerModule.GetService<IHoh_Project_ReportService>(); } }
    }
 }
