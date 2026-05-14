/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹V_Hoh_Project_DataRepository编写代码
 */
using Hncdi.HeatOfHydration.IRepositories;
using VolPro.Core.BaseProvider;
using VolPro.Core.EFDbContext;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace Hncdi.HeatOfHydration.Repositories
{
    public partial class V_Hoh_Project_DataRepository : RepositoryBase<V_Hoh_Project_Data> , IV_Hoh_Project_DataRepository
    {
    public V_Hoh_Project_DataRepository(ServiceDbContext dbContext)
    : base(dbContext)
    {

    }
    public static IV_Hoh_Project_DataRepository Instance
    {
      get {  return AutofacContainerModule.GetService<IV_Hoh_Project_DataRepository>(); } }
    }
}
