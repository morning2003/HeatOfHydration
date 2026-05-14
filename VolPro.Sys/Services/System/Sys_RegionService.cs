/*
 *Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下Sys_RegionService与ISys_RegionService中编写
 */
using VolPro.Sys.IRepositories;
using VolPro.Sys.IServices;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace VolPro.Sys.Services
{
    public partial class Sys_RegionService : ServiceBase<Sys_Region, ISys_RegionRepository>
    , ISys_RegionService, IDependency
    {
    public Sys_RegionService(ISys_RegionRepository repository)
    : base(repository)
    {
    Init(repository);
    }
    public static ISys_RegionService Instance
    {
      get { return AutofacContainerModule.GetService<ISys_RegionService>(); } }
    }
 }
