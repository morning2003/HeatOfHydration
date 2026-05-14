/*
 *Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下V_Hoh_Project_DataService与IV_Hoh_Project_DataService中编写
 */
using Hncdi.HeatOfHydration.IRepositories;
using Hncdi.HeatOfHydration.IServices;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace Hncdi.HeatOfHydration.Services
{
    public partial class V_Hoh_Project_DataService : ServiceBase<V_Hoh_Project_Data, IV_Hoh_Project_DataRepository>
    , IV_Hoh_Project_DataService, IDependency
    {
    public static IV_Hoh_Project_DataService Instance
    {
      get { return AutofacContainerModule.GetService<IV_Hoh_Project_DataService>(); } }
    }
 }
