/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹TestDbRepository编写代码
 */
using VolPro.DbTest.IRepositories;
using VolPro.Core.BaseProvider;
using VolPro.Core.EFDbContext;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace VolPro.DbTest.Repositories
{
    public partial class TestDbRepository : RepositoryBase<TestDb> , ITestDbRepository
    {
    public TestDbRepository(TestDbContext dbContext)
    : base(dbContext)
    {

    }
    public static ITestDbRepository Instance
    {
      get {  return AutofacContainerModule.GetService<ITestDbRepository>(); } }
    }
}
