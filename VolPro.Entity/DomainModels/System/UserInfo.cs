using VolPro.Entity.SystemModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolPro.Entity.DomainModels
{
    public class UserInfo
    {
        public int User_Id { get; set; }
        /// <summary>
        /// 多个角色ID
        /// </summary>
        public int Role_Id { get; set; }
        public string RoleName { get; set; }
        public string UserName { get; set; }
        public string UserTrueName { get; set; }
        public int  Enable { get; set; }
        /// <summary>
        /// 部门id
        /// </summary>
        public int DeptId { get; set; }
        /// <summary>
        /// 岗位id
        /// </summary>
        public List<Guid> PostIds { get; set; }

        public List<Guid> DeptIds { get; set; }


        public int[] RoleIds { get; set; }
        public string Token { get; set; }


        /// <summary>
        /// 2023.12.10实现租户字段过滤
        /// 租户值, 请在UserContext类GetUserInfo方法中设置TenancyValue的值
        /// </summary>
        public string TenancyValue { get; set; }


        public string DefaultUrl { get; set; }
    }
}
