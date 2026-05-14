/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹Hncdi_ProjectRepository编写代码
 */
using Hncdi.HeatOfHydration.IRepositories;
using VolPro.Core.BaseProvider;
using VolPro.Core.EFDbContext;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace Hncdi.HeatOfHydration.Repositories
{
    public partial class Hncdi_ProjectRepository : RepositoryBase<Hncdi_Project> , IHncdi_ProjectRepository
    {
    public Hncdi_ProjectRepository(ServiceDbContext dbContext)
    : base(dbContext)
    {

    }
    public static IHncdi_ProjectRepository Instance
    {
      get {  return AutofacContainerModule.GetService<IHncdi_ProjectRepository>(); } }
    }
}
