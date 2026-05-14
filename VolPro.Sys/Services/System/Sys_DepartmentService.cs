/*
 *Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下Sys_DepartmentService与ISys_DepartmentService中编写
 */
using VolPro.Sys.IRepositories;
using VolPro.Sys.IServices;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace VolPro.Sys.Services
{
    public partial class Sys_DepartmentService : ServiceBase<Sys_Department, ISys_DepartmentRepository>
    , ISys_DepartmentService, IDependency
    {
    public Sys_DepartmentService(ISys_DepartmentRepository repository)
    : base(repository)
    {
    Init(repository);
    }
    public static ISys_DepartmentService Instance
    {
      get { return AutofacContainerModule.GetService<ISys_DepartmentService>(); } }
    }
 }
