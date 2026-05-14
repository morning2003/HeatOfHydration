/*
*所有关于Hoh_Project类的业务代码接口应在此处编写
*/
using VolPro.Core.BaseProvider;
using VolPro.Entity.DomainModels;
using VolPro.Core.Utilities;
using System.Linq.Expressions;
namespace Hncdi.HeatOfHydration.IServices
{
    public partial interface IHoh_ProjectService
    {
        WebResponseContent GetProjectDataInfo(long hohProjectCode);
        
    }
 }
