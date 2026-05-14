using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.BaseProvider;
using VolPro.Entity.DomainModels;
using VolPro.Core.Extensions.AutofacManager;
namespace VolPro.Builder.IRepositories
{
    public partial interface ISys_TableInfoRepository : IDependency,IRepository<Sys_TableInfo>
    {
    }
}

