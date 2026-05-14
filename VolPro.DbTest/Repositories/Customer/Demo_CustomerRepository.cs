/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹Demo_CustomerRepository编写代码
 */
using VolPro.DbTest.IRepositories;
using VolPro.Core.BaseProvider;
using VolPro.Core.EFDbContext;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace VolPro.DbTest.Repositories
{
    public partial class Demo_CustomerRepository : RepositoryBase<Demo_Customer> , IDemo_CustomerRepository
    {
    public Demo_CustomerRepository(SysDbContext dbContext)
    : base(dbContext)
    {

    }
    public static IDemo_CustomerRepository Instance
    {
      get {  return AutofacContainerModule.GetService<IDemo_CustomerRepository>(); } }
    }
}
