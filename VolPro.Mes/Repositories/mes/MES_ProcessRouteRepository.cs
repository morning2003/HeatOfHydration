/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹MES_ProcessRouteRepository编写代码
 */
using VolPro.MES.IRepositories;
using VolPro.Core.BaseProvider;
using VolPro.Core.EFDbContext;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace VolPro.MES.Repositories
{
    public partial class MES_ProcessRouteRepository : RepositoryBase<MES_ProcessRoute> , IMES_ProcessRouteRepository
    {
    public MES_ProcessRouteRepository(ServiceDbContext dbContext)
    : base(dbContext)
    {

    }
    public static IMES_ProcessRouteRepository Instance
    {
      get {  return AutofacContainerModule.GetService<IMES_ProcessRouteRepository>(); } }
    }
}
