/*
 *Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下MES_ProductionPlanChangeRecordService与IMES_ProductionPlanChangeRecordService中编写
 */
using VolPro.MES.IRepositories;
using VolPro.MES.IServices;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace VolPro.MES.Services
{
    public partial class MES_ProductionPlanChangeRecordService : ServiceBase<MES_ProductionPlanChangeRecord, IMES_ProductionPlanChangeRecordRepository>
    , IMES_ProductionPlanChangeRecordService, IDependency
    {
    public static IMES_ProductionPlanChangeRecordService Instance
    {
      get { return AutofacContainerModule.GetService<IMES_ProductionPlanChangeRecordService>(); } }
    }
 }
