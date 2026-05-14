/*
 *Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下MES_ProductOutboundService与IMES_ProductOutboundService中编写
 */
using VolPro.MES.IRepositories;
using VolPro.MES.IServices;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace VolPro.MES.Services
{
    public partial class MES_ProductOutboundService : ServiceBase<MES_ProductOutbound, IMES_ProductOutboundRepository>
    , IMES_ProductOutboundService, IDependency
    {
    public static IMES_ProductOutboundService Instance
    {
      get { return AutofacContainerModule.GetService<IMES_ProductOutboundService>(); } }
    }
 }
