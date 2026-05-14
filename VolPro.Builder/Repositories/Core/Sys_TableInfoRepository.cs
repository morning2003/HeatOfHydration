using VolPro.Builder.IRepositories;
using VolPro.Core.BaseProvider;
using VolPro.Core.EFDbContext;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace VolPro.Builder.Repositories
{
    public partial class Sys_TableInfoRepository : RepositoryBase<Sys_TableInfo>, ISys_TableInfoRepository
    {
        public Sys_TableInfoRepository(SysDbContext dbContext)
        : base(dbContext)
        {

        }
        public static ISys_TableInfoRepository GetService
        {
            get { return AutofacContainerModule.GetService<ISys_TableInfoRepository>(); }
        }
    }
}

