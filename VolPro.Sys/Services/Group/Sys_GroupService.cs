/*
 *Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下Sys_GroupService与ISys_GroupService中编写
 */
using VolPro.Sys.IRepositories;
using VolPro.Sys.IServices;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace VolPro.Sys.Services
{
    public partial class Sys_GroupService : ServiceBase<Sys_Group, ISys_GroupRepository>
    , ISys_GroupService, IDependency
    {
    public Sys_GroupService(ISys_GroupRepository repository)
    : base(repository)
    {
    Init(repository);
    }
    public static ISys_GroupService Instance
    {
      get { return AutofacContainerModule.GetService<ISys_GroupService>(); } }
    }
 }
