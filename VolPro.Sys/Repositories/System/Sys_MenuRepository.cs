using VolPro.Sys.IRepositories;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Core.EFDbContext;
using VolPro.Entity.DomainModels;

namespace VolPro.Sys.Repositories
{
    public partial class Sys_MenuRepository : RepositoryBase<Sys_Menu>, ISys_MenuRepository
    {
        public Sys_MenuRepository(SysDbContext dbContext)
        : base(dbContext)
        {

        }
        public static ISys_MenuRepository Instance
        {
            get { return AutofacContainerModule.GetService<ISys_MenuRepository>(); }
        }
    }
}

