using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using VolPro.Core.CacheManager;
using VolPro.Core.DBManager;
using VolPro.Core.Extensions;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Core.ManageUser;
using VolPro.Core.Services;
using VolPro.Entity.DomainModels;
using CC = VolPro.Core.CacheManager;

namespace VolPro.Core.UserManager
{
    public static class RoleContext
    {

        private static object _RoleObj = new object();
        private static string _RoleVersionn = "";
        public const string Key = "inernalRole";

        static RoleContext()
        {
            InitRoleFields();
        }

        private static List<RoleFields> RoleFieldsList = new List<RoleFields>();
        /// <summary>
        /// 字段权限
        /// </summary>
        public static void InitRoleFields()
        {
            RoleFieldsList = DBServerProvider.DbContext.Set<Sys_RoleFields>()
                .Where(x => !string.IsNullOrEmpty(x.AuthFields))
                .Select(s => new { s.RoleId, s.TableName, s.AuthFields })
                .ToList()
                .Select(s => new RoleFields()
                {
                    RoleId = s.RoleId,
                    TableName = s.TableName,
                    LowerName = s.TableName?.ToLower(),
                    Fields = (s.AuthFields ?? "").Split(",")
                }).ToList();
        }

        private static object _obj_fields = new object();

        /// <summary>
        /// 获取当前用户表的权限字段
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string[] GetCurrentRoleAuthFields(string tableName)
        {
            if (UserContext.Current.IsSuperAdmin)
            {
                return new string[] { };
            }
            var roleId = UserContext.Current.RoleIds;
            return RoleFieldsList.Where(c => roleId.Contains(c.RoleId) && c.TableName == tableName).Select(s => s.Fields).FirstOrDefault() ?? new string[] { };
        }
        /// <summary>
        /// 获取当前用户所有表字段权限
        /// </summary>
        /// <returns></returns>
        public static object GetCurrentRoleAllAuthFields()
        {
            if (UserContext.Current.IsSuperAdmin)
            {
                return new string[] { };
            }
            string[] tables = UserContext.Current.Permissions.Where(c => c.TableName != "").Select(s => s.TableName).ToArray();
            var roleId = UserContext.Current.RoleIds;
            return RoleFieldsList.Where(c => tables.Contains(c.LowerName) && roleId.Contains(c.RoleId))
                .Select(s => new { name = s.TableName, s.Fields }).ToList();
        }

        public static void UpdateRoleFields(Sys_RoleFields roleFields)
        {
            lock (_obj_fields)
            {
                var data = RoleFieldsList.Where(c => c.RoleId == roleFields.RoleId && c.TableName == roleFields.TableName).FirstOrDefault();
                if (data != null)
                {
                    if (string.IsNullOrEmpty(roleFields.AuthFields))
                    {
                        RoleFieldsList.Remove(data);
                    }
                    else
                    {
                        data.Fields = (roleFields.AuthFields ?? "").Split(",");
                    }
                    return;
                }

                if (string.IsNullOrEmpty(roleFields.AuthFields))
                {
                    return;
                }
                RoleFieldsList.Add(new RoleFields()
                {
                    RoleId = roleFields.RoleId,
                    TableName = roleFields.TableName,
                    LowerName = roleFields.TableName?.ToLower(),
                    Fields = (roleFields.AuthFields ?? "").Split(",")
                });

            }
        }

        private static List<RoleNodes> _roles { get; set; }
        public static List<RoleNodes> GetAllRoleId()
        {
            CC.ICacheService cacheService = AutofacContainerModule.GetService<CC.ICacheService>();
            //每次比较缓存是否更新过，如果更新则重新获取数据
            if (_roles != null && _RoleVersionn == cacheService.Get(Key))
            {
                return _roles;
            }
            lock (_RoleObj)
            {
                if (_RoleVersionn != "" && _roles != null && _RoleVersionn == cacheService.Get(Key)) return _roles;
                _roles = DBServerProvider.DbContext
                  .Set<Sys_Role>()
                  .ToList()
                   .Select(s => new RoleNodes()
                   {
                       Id = s.Role_Id,
                       ParentId = s.ParentId,
                       RoleName = s.RoleName,
                       DbServiceId = s.DbServiceId ?? Guid.Empty,
                       AuthData = s.DatAuth.GetInt()
                   })
             .ToList();

                string cacheVersion = cacheService.Get(Key);
                if (string.IsNullOrEmpty(cacheVersion))
                {
                    cacheVersion = DateTime.Now.ToString("yyyyMMddHHMMssfff");
                    cacheService.Add(Key, cacheVersion);
                }
                else
                {
                    _RoleVersionn = cacheVersion;
                }
            }
            return _roles;
        }

        public static void Refresh()
        {
            AutofacContainerModule.GetService<CC.ICacheService>().Remove(Key);
        }
        /// <summary>
        /// 
        /// 获取当前角色下的所有角色(包括自己的角色)
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public static List<RoleNodes> GetAllChildren(int roleId)
        {
            if (roleId <= 0) return new List<RoleNodes>() { };
            var roles = GetAllRoleId();
            if (UserContext.IsRoleIdSuperAdmin(roleId)) return roles;

            var list = GetChildren(roles, roleId);
            //if (list.Any(x => x.Id == roleId))
            //{
            //    return list.Where(x => x.Id != roleId).ToList();
            //}
            return list;
        }

        public static List<RoleNodes> GetAllChildren(int[] roleIds)
        {
            return roleIds.SelectMany(x => GetAllChildren(x)).Distinct().ToList();
        }


        public static List<int> GetAllChildrenIds(int[] roleIds)
        {
            return roleIds.SelectMany(x => GetAllChildren(x).Select(x => x.Id)).Distinct().ToList();
        }
        private static List<RoleNodes> GetChildren(List<RoleNodes> roles, int roleId)
        {
            List<RoleNodes> rolesChildren = roles.Where(x => x.Id == roleId).Distinct().ToList();

            for (int i = 0; i < rolesChildren.Count; i++)
            {
                RoleNodes node = rolesChildren[i];
                var children = roles.Where(x => x.ParentId == node.Id && !rolesChildren.Any(c => c.Id == x.Id)).Distinct().ToList();
                rolesChildren.AddRange(children);
            }
            return rolesChildren;
        }
        /// <summary>
        /// 获取当前角色下的所有用户
        /// </summary>
        /// <returns></returns>
        public static ISugarQueryable<int> GetCurrentAllChildUser()
        {
            var roles = GetAllChildrenIds(UserContext.Current.RoleIds);
            if (roles == null)
            {
                throw new Exception("未获取到当前角色");
            }

            return DBServerProvider.DbContext
                  .Set<Sys_User>()
                  .Where(u => SqlFunc.Subqueryable<Sys_UserRole>().Where(x => roles.Contains(x.RoleId) && x.Enable == 1 && x.UserId == u.User_Id).Any()).Select(s => s.User_Id);
        }

        public static IEnumerable<RoleNodes> GetRoles(Func<RoleNodes, bool> where)
        {
            return GetAllRoleId().Where(where);
        }
    }
    public class RoleNodes
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string RoleName { get; set; }

        public Guid DbServiceId { get; set; }

        /// <summary>
        /// 数据权限
        /// </summary>
        public int AuthData { get; set; }
    }

    public class RoleFields
    {
        public int RoleId { get; set; }

        public string TableName { get; set; }
        public string LowerName { get; set; }

        public string[] Fields { get; set; }
    }
}
