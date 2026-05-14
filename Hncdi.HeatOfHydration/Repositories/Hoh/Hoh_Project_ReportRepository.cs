/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹Hoh_Project_ReportRepository编写代码
 */
using Hncdi.HeatOfHydration.IRepositories;
using VolPro.Core.BaseProvider;
using VolPro.Core.EFDbContext;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace Hncdi.HeatOfHydration.Repositories
{
    public partial class Hoh_Project_ReportRepository : RepositoryBase<Hoh_Project_Report> , IHoh_Project_ReportRepository
    {
    public Hoh_Project_ReportRepository(ServiceDbContext dbContext)
    : base(dbContext)
    {

    }
    public static IHoh_Project_ReportRepository Instance
    {
      get {  return AutofacContainerModule.GetService<IHoh_Project_ReportRepository>(); } }
    }
}
