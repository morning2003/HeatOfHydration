/*
 *Author：jxx
 *Contact：283591387@qq.com
 *Date：2018-07-01
 * 此代码由框架生成，请勿随意更改
 */
using VolPro.Sys.IRepositories;
using VolPro.Sys.IServices;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace VolPro.Sys.Services
{
    public partial class Sys_UserService : ServiceBase<Sys_User, ISys_UserRepository>, ISys_UserService, IDependency
    {
        public Sys_UserService(ISys_UserRepository repository)
             : base(repository) 
        { 
           Init(repository);
        }
        public static ISys_UserService Instance
        {
           get { return AutofacContainerModule.GetService<ISys_UserService>(); }
        }
    }
}

