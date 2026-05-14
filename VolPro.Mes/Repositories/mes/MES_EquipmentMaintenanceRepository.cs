/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹MES_EquipmentMaintenanceRepository编写代码
 */
using VolPro.MES.IRepositories;
using VolPro.Core.BaseProvider;
using VolPro.Core.EFDbContext;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace VolPro.MES.Repositories
{
    public partial class MES_EquipmentMaintenanceRepository : RepositoryBase<MES_EquipmentMaintenance> , IMES_EquipmentMaintenanceRepository
    {
    public MES_EquipmentMaintenanceRepository(ServiceDbContext dbContext)
    : base(dbContext)
    {

    }
    public static IMES_EquipmentMaintenanceRepository Instance
    {
      get {  return AutofacContainerModule.GetService<IMES_EquipmentMaintenanceRepository>(); } }
    }
}
