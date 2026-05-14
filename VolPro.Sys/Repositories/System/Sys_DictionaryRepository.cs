/*
 *Author：jxx
 *Contact：283591387@qq.com
 *Date：2018-07-01
 * 此代码由框架生成，请勿随意更改
 */
using VolPro.Sys.IRepositories;
using VolPro.Core.BaseProvider;
using VolPro.Core.EFDbContext;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace VolPro.Sys.Repositories
{
    public partial class Sys_DictionaryRepository : RepositoryBase<Sys_Dictionary>, ISys_DictionaryRepository
    {
        public Sys_DictionaryRepository(SysDbContext dbContext)
        : base(dbContext)
        {

        }
        public static ISys_DictionaryRepository Instance
        {
            get { return AutofacContainerModule.GetService<ISys_DictionaryRepository>(); }
        }
    }
}

