/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹Hoh_Project_PointRepository编写代码
 */
using Hncdi.HeatOfHydration.IRepositories;
using VolPro.Core.BaseProvider;
using VolPro.Core.EFDbContext;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace Hncdi.HeatOfHydration.Repositories
{
    public partial class Hoh_Project_PointRepository : RepositoryBase<Hoh_Project_Point> , IHoh_Project_PointRepository
    {
    public Hoh_Project_PointRepository(ServiceDbContext dbContext)
    : base(dbContext)
    {

    }
    public static IHoh_Project_PointRepository Instance
    {
      get {  return AutofacContainerModule.GetService<IHoh_Project_PointRepository>(); } }
    }
}
