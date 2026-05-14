using VolPro.Sys.IRepositories;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Core.EFDbContext;
using VolPro.Entity.DomainModels;

namespace VolPro.Sys.Repositories
{
    public partial class Sys_LogRepository : RepositoryBase<Sys_Log>, ISys_LogRepository
    {
        public Sys_LogRepository(SysDbContext dbContext)
        : base(dbContext)
        {

        }
        public static ISys_LogRepository GetService
        {
            get { return AutofacContainerModule.GetService<ISys_LogRepository>(); }
        }
    }
}

