using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VolPro.Core;
using VolPro.Core.Configuration;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.Enums;
using VolPro.Core.Extensions;
using VolPro.Core.ManageUser;
using VolPro.Core.Services;
using VolPro.Core.UserManager;
using VolPro.Core.Utilities;
using VolPro.Entity.DomainModels;
using VolPro.Sys.IRepositories;

namespace VolPro.Sys.Services
{
    public partial class Sys_UserService
    {
        private Microsoft.AspNetCore.Http.HttpContext _context;
        private ISys_UserRepository _repository;

        [ActivatorUtilitiesConstructor]
        public Sys_UserService(IHttpContextAccessor httpContextAccessor, ISys_UserRepository repository)
            : base(repository)
        {
            _context = httpContextAccessor.HttpContext;
            _repository = repository;
        }
        WebResponseContent webResponse = new WebResponseContent();
        /// <summary> 
        /// WebApi登陆
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <param name="verificationCode"></param> 
        /// <returns></returns>
        public async Task<WebResponseContent> Login(LoginInfo loginInfo, bool verificationCode = true)
        {
            WebResponseContent responseContent = new WebResponseContent();
            string msg = string.Empty;
            //   2020.06.12增加验证码
            IMemoryCache memoryCache = _context.GetService<IMemoryCache>();
            string cacheCode = (memoryCache.Get(loginInfo.UUID) ?? "").ToString();
            if (string.IsNullOrEmpty(cacheCode))
            {
                return responseContent.Error("验证码已失效".Translator());
            }
            if (cacheCode.ToLower() != loginInfo.VerificationCode.ToLower())
            {
                memoryCache.Remove(loginInfo.UUID);
                return responseContent.Error("验证码不正确".Translator());
            }
            try
            {
                Sys_User user = await repository.FindAsIQueryable(x => x.UserName == loginInfo.UserName)
                    .FirstOrDefaultAsync();

                if (user == null || loginInfo.Password.Trim().EncryptDES(AppSetting.Secret.User) != (user.UserPwd ?? ""))
                    return webResponse.Error(ResponseType.LoginError);
                int expir = UserContext.MenuType == 1 ? 43200 : AppSetting.ExpMinutes;
                string token = JwtHelper.IssueJwt(new UserInfo()
                {
                    User_Id = user.User_Id,
                    UserName = user.UserName,
                    Role_Id = user.Role_Id ?? 0,
                }, expir);
                user.Token = token;
                string accessToken = null;
                if (AppSetting.FileAuth)
                {
                    expir = expir + 30;
                    string dt = DateTime.Now.AddMinutes(expir).ToString("yyyy-MM-dd HH:mm");
                    accessToken = $"{user.User_Id}_{dt}".EncryptDES(AppSetting.Secret.User);
                    _context.GetService<Core.CacheManager.ICacheService>().Add(accessToken, dt, expir);
                }
                webResponse.Data = new { token, userName = user.UserTrueName, img = user.HeadImageUrl, accessToken ,user.DefaultUrl};
                repository.Update(user, x => x.Token, true);
                UserContext.Current.LogOut(user.User_Id);

                loginInfo.Password = string.Empty;

                return webResponse.OK(ResponseType.LoginSuccess);
            }
            catch (Exception ex)
            {
                msg = ex.Message + ex.StackTrace;
                if (_context.GetService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>().IsDevelopment())
                {
                    throw new Exception(ex.Message + ex.StackTrace);
                }
                return webResponse.Error(ResponseType.ServerError);
            }
            finally
            {
                memoryCache.Remove(loginInfo.UUID);
                Logger.Info(LoggerType.Login, loginInfo.Serialize(), webResponse.Message, msg);
            }
        }

        /// <summary>
        ///当token将要过期时，提前置换一个新的token
        /// </summary>
        /// <returns></returns>
        public async Task<WebResponseContent> ReplaceToken()
        {
            await Task.CompletedTask;
            return new WebResponseContent() { };
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<WebResponseContent> ModifyPwd(string oldPwd, string newPwd)
        {
            oldPwd = oldPwd?.Trim();
            newPwd = newPwd?.Trim();
            string message = "";
            WebResponseContent webResponse = new WebResponseContent();
            try
            {
                if (string.IsNullOrEmpty(oldPwd)) return webResponse.Error("旧密码不能为空".Translator());
                if (string.IsNullOrEmpty(newPwd)) return webResponse.Error("新密码不能为空".Translator());
                if (newPwd.Length < 6) return webResponse.Error("密码不能少于6位".Translator());

                int userId = UserContext.Current.UserId;
                string userCurrentPwd = await base.repository.FindFirstAsync(x => x.User_Id == userId, s => s.UserPwd);

                string _oldPwd = oldPwd.EncryptDES(AppSetting.Secret.User);
                if (_oldPwd != userCurrentPwd) return webResponse.Error("旧密码不正确".Translator());

                string _newPwd = newPwd.EncryptDES(AppSetting.Secret.User);
                if (userCurrentPwd == _newPwd) return webResponse.Error("新密码不能与旧密码相同".Translator());


                repository.Update(new Sys_User
                {
                    User_Id = userId,
                    UserPwd = _newPwd,
                    LastModifyPwdDate = DateTime.Now
                }, x => new { x.UserPwd, x.LastModifyPwdDate }, true);

                webResponse.OK("密码修改成功".Translator());
            }
            catch (Exception ex)
            {
                message = ex.Message;
                webResponse.Error("服务器处理出现异常".Translator());
            }
            finally
            {
                if (message == "")
                {
                    Logger.OK(LoggerType.ApiModifyPwd, "密码修改成功".Translator());
                }
                else
                {
                    Logger.Error(LoggerType.ApiModifyPwd, message);
                }
            }
            return webResponse;
        }
        /// <summary>
        /// 个人中心获取当前用户信息
        /// </summary>
        /// <returns></returns>
        public async Task<WebResponseContent> GetCurrentUserInfo()
        {
            var data = await base.repository
                .FindAsIQueryable(x => x.User_Id == UserContext.Current.UserId)
                .Select(s => new
                {
                    s.UserName,
                    s.UserTrueName,
                    //  s.Address,
                    s.PhoneNo,
                    //  s.Email,
                    s.Remark,
                    s.Gender,
                    // s.RoleName,
                    s.HeadImageUrl,
                    s.CreateDate
                })
                .FirstOrDefaultAsync();
            return webResponse.OK(null, data);
        }

        /// <summary>
        /// 设置固定排序方式及显示用户过滤
        /// </summary>
        /// <param name="pageData"></param>
        /// <returns></returns>
        public override PageGridData<Sys_User> GetPageData(PageDataOptions pageData)
        {
            ////var roleId = new int[] {};
            //////树形菜单传查询角色下所有用户
            ////if (pageData.Value != null)
            ////{
            ////    roleId = new int[] { pageData.Value.ToString().GetInt() };
            ////}
            //QueryRelativeExpression = (ISugarQueryable<Sys_User> queryable) =>
            //{
            //    //显示角色与子角色下的数据
            //    var serviceId = UserContext.CurrentServiceId;
            //    var roleIds = RoleContext.GetAllChildrenIds(UserContext.Current.RoleIds);
            //    var roleQuery = repository.DbContext.Set<Sys_UserRole>().Where(x => x.Enable == 1 && roleIds.Contains(x.RoleId));
            //    if (UserContext.Current.IsSuperAdmin)
            //    {
            //        return queryable;
            //        //  return queryable.Where(x => x.User_Id == UserContext.Current.UserId || roleQuery.Any(c => c.RoleId == x.Role_Id));
            //    }
            //    return queryable.Where(x => roleQuery.Any(c => c.UserId == x.User_Id));
            //};
            //显示同一个数据库的用户
            var data = base.GetPageData(pageData);
            foreach (var item in data.rows)
            {
                item.Token = null;
            }
            return data;
        }

        /// <summary>
        /// 新建用户，根据实际情况自行处理
        /// </summary>
        /// <param name="saveModel"></param>
        /// <returns></returns>
        public override WebResponseContent Add(SaveModel saveModel)
        {
            WebResponseContent responseData = new WebResponseContent();
            base.AddOnExecute = (SaveModel userModel) =>
            {
                return responseData.OK();
            };


            ///生成6位数随机密码
            string pwd = 6.GenerateRandomNumber();
            //在AddOnExecuting之前已经对提交的数据做过验证是否为空
            base.AddOnExecuting = (Sys_User user, object obj) =>
            {
                user.UserName = user.UserName.Trim();
                if (repository.Exists(x => x.UserName == user.UserName))
                    return responseData.Error("用户名已经被注册".Translator());
                user.UserPwd = pwd.EncryptDES(AppSetting.Secret.User);
                //设置默认头像
                return responseData.OK();
            };

            base.AddOnExecuted = (Sys_User user, object list) =>
            {
                var roleIds = user.RoleIds?.Split(",").Select(s => s.GetInt()).Where(x => x > 1).ToArray();
                SaveRole(roleIds, user.User_Id);
                var deptIds = user.DeptIds?.Split(",").Select(s => s.GetGuid()).Where(x => x != null).Select(s => (Guid)s).ToArray();
                SaveDepartment(deptIds, user.User_Id);
                var postIds = user.PostId?.Split(",").Select(s => s.GetGuid()).Where(x => x != null).Select(s => (Guid)s).ToArray();
                SavePost(postIds, user.User_Id);
                return responseData.OK("用户新建成功.帐号{$ts}密码{$ts}".TranslatorFormat(user.UserName, pwd));
            };
            return base.Add(saveModel); ;
        }

        /// <summary>
        /// 删除用户拦截过滤
        /// 用户被删除后同时清空对应缓存
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="delList"></param>
        /// <returns></returns>
        public override WebResponseContent Del(object[] keys, bool delList = false)
        {
            base.DelOnExecuting = (object[] ids) =>
            {
                int[] userIds = ids.Select(x => Convert.ToInt32(x)).ToArray();
                return new WebResponseContent().OK();
            };
            base.DelOnExecuted = (object[] userIds) =>
            {
                var objKeys = userIds.Select(x => x.GetInt().GetUserIdKey());
                base.CacheContext.RemoveAll(objKeys);
                return new WebResponseContent() { Status = true };
            };
            return base.Del(keys, delList);
        }

        /// <summary>
        /// 保存角色
        /// </summary>
        /// <param name="roleIds"></param>
        /// <param name="userId"></param>
        public void SaveRole(int[] roleIds, int userId)
        {
            if (userId <= 0)
            {
                return;
            }
            if (roleIds == null)
            {
                roleIds = new int[] { };
            }

            //如果需要判断当前角色是否越权，再调用一下获取当前角色下的所有子角色判断即可

            var roles = repository.DbContext.Set<Sys_UserRole>().Where(x => x.UserId == userId)
              .Select(s => new { s.RoleId, s.Enable, s.Id })
              .ToList();
            ////没有设置角色
            //if (roleIds.Length == 0 && roles.Exists(x => x.Enable == 1))
            //{
            //    return;
            //}

            UserInfo user = UserContext.Current.UserInfo;
            //新设置的角色 
            var add = roleIds.Where(x => !roles.Exists(r => r.RoleId == x)).Select(s => new Sys_UserRole()
            {
                Id = Guid.NewGuid(),
                RoleId = s,
                UserId = userId,
                Enable = 1,
                CreateDate = DateTime.Now,
                Creator = user.UserTrueName,
                CreateID = user.User_Id
            }).ToList();

            //删除的角色 
            var update = roles.Where(x => !roleIds.Contains(x.RoleId) && x.Enable == 1).Select(s => new Sys_UserRole()
            {
                Id = s.Id,
                Enable = 0,
                ModifyDate = DateTime.Now,
                Modifier = user.UserTrueName,
                ModifyID = user.User_Id
            }).ToList();

            //之前设置过的角色重新分配 
            update.AddRange(roles.Where(x => roleIds.Contains(x.RoleId) && x.Enable != 1).Select(s => new Sys_UserRole()
            {
                Id = s.Id,
                Enable = 1,
                ModifyDate = DateTime.Now,
                Modifier = user.UserTrueName,
                ModifyID = user.User_Id
            }).ToList());
            repository.AddRange(add);

            repository.UpdateRange(update, x => new { x.Enable, x.ModifyDate, x.Modifier, x.ModifyID });
            repository.SaveChanges();
        }

        /// <summary>
        /// 保存部门
        /// </summary>
        /// <param name="deptIds"></param>
        /// <param name="userId"></param>
        public void SaveDepartment(Guid[] deptIds, int userId)
        {

            if (userId <= 0)
            {
                return;
            }
            if (deptIds == null)
            {
                deptIds = new Guid[] { };
            }

            //如果需要判断当前角色是否越权，再调用一下获取当前部门下的所有子角色判断即可

            var roles = repository.DbContext.Set<Sys_UserDepartment>().Where(x => x.UserId == userId)
              .Select(s => new { s.DepartmentId, s.Enable, s.Id })
              .ToList();
            //没有设置部门
            if (deptIds.Length == 0 && !roles.Exists(x => x.Enable == 1))
            {
                return;
            }

            UserInfo user = UserContext.Current.UserInfo;
            //新设置的部门
            var add = deptIds.Where(x => !roles.Exists(r => r.DepartmentId == x)).Select(s => new Sys_UserDepartment()
            {
                Id = Guid.NewGuid(),
                DepartmentId = s,
                UserId = userId,
                Enable = 1,
                CreateDate = DateTime.Now,
                Creator = user.UserTrueName,
                CreateID = user.User_Id
            }).ToList();

            //删除的部门
            var update = roles.Where(x => !deptIds.Contains(x.DepartmentId) && x.Enable == 1).Select(s => new Sys_UserDepartment()
            {
                Id = s.Id,
                Enable = 0,
                ModifyDate = DateTime.Now,
                Modifier = user.UserTrueName,
                ModifyID = user.User_Id
            }).ToList();

            //之前设置过的部门重新分配 
            update.AddRange(roles.Where(x => deptIds.Contains(x.DepartmentId) && x.Enable != 1).Select(s => new Sys_UserDepartment()
            {
                Id = s.Id,
                Enable = 1,
                ModifyDate = DateTime.Now,
                Modifier = user.UserTrueName,
                ModifyID = user.User_Id
            }).ToList());
            repository.AddRange(add);

            repository.UpdateRange(update, x => new { x.Enable, x.ModifyDate, x.Modifier, x.ModifyID });
            repository.SaveChanges();
        }
        /// <summary>
        /// 保存岗位
        /// </summary>
        /// <param name="postIds"></param>
        /// <param name="userId"></param>
        public void SavePost(Guid[] postIds, int userId)
        {

            if (userId <= 0)
            {
                return;
            }
            if (postIds == null)
            {
                postIds = new Guid[] { };
            }

            var roles = repository.DbContext.Set<Sys_UserPost>().Where(x => x.UserId == userId)
              .Select(s => new { s.PostId, s.Enable, s.Id })
              .ToList();
            //没有设置部门
            if (postIds.Length == 0 && !roles.Exists(x => x.Enable == 1))
            {
                return;
            }

            UserInfo user = UserContext.Current.UserInfo;
            //新设置的部门
            var add = postIds.Where(x => !roles.Exists(r => r.PostId == x)).Select(s => new Sys_UserPost()
            {
                Id = Guid.NewGuid(),
                PostId = s,
                UserId = userId,
                Enable = 1,
                CreateDate = DateTime.Now,
                Creator = user.UserTrueName,
                CreateID = user.User_Id
            }).ToList();

            //删除的部门
            var update = roles.Where(x => !postIds.Contains(x.PostId) && x.Enable == 1).Select(s => new Sys_UserPost()
            {
                Id = s.Id,
                Enable = 0,
                ModifyDate = DateTime.Now,
                Modifier = user.UserTrueName,
                ModifyID = user.User_Id
            }).ToList();

            //之前设置过的部门重新分配 
            update.AddRange(roles.Where(x => postIds.Contains(x.PostId) && x.Enable != 1).Select(s => new Sys_UserPost()
            {
                Id = s.Id,
                Enable = 1,
                ModifyDate = DateTime.Now,
                Modifier = user.UserTrueName,
                ModifyID = user.User_Id
            }).ToList());
            repository.AddRange(add);

            repository.UpdateRange(update, x => new { x.Enable, x.ModifyDate, x.Modifier, x.ModifyID });
            repository.SaveChanges();
        }
        /// <summary>
        /// 修改用户拦截过滤
        /// 
        /// </summary>
        /// <param name="saveModel"></param>
        /// <returns></returns>
        public override WebResponseContent Update(SaveModel saveModel)
        {
            UserInfo userInfo = UserContext.Current.UserInfo;
            base.UpdateOnExecuting = (Sys_User user, object obj1, object obj2, List<object> list) =>
            {
                var _user = repository.Find(x => x.User_Id == user.User_Id,
                    s => new { s.UserName, s.UserPwd })
                    .FirstOrDefault();
                user.UserName = _user.UserName;
                //Sys_User实体的UserPwd用户密码字段的属性不是编辑，此处不会修改密码。但防止代码生成器将密码字段的修改成了可编辑造成密码被修改
                user.UserPwd = _user.UserPwd;
                return webResponse.OK();
            };
            //用户信息被修改后，将用户的缓存信息清除
            base.UpdateOnExecuted = (Sys_User user, object obj1, object obj2, List<object> List) =>
            {
                base.CacheContext.Remove(user.User_Id.GetUserIdKey());

                var roleIds = user.RoleIds?.Split(",").Select(s => s.GetInt()).Where(x => x > 1).ToArray();
                SaveRole(roleIds, user.User_Id);


                var deptIds = user.DeptIds?.Split(",").Select(s => s.GetGuid()).Where(x => x != null).Select(s => (Guid)s).ToArray();
                SaveDepartment(deptIds, user.User_Id);
                var postIds = user.PostId?.Split(",").Select(s => s.GetGuid()).Where(x => x != null).Select(s => (Guid)s).ToArray();
                SavePost(postIds, user.User_Id);
                return new WebResponseContent(true);
            };
            return base.Update(saveModel);
        }

        /// <summary>
        /// 导出处理
        /// </summary>
        /// <param name="pageData"></param>
        /// <returns></returns>
        public override WebResponseContent Export(PageDataOptions pageData)
        {
            //限定只能导出当前角色能看到的所有用户
            QueryRelativeExpression = (ISugarQueryable<Sys_User> queryable) =>
            {
                if (UserContext.Current.IsSuperAdmin) return queryable;
                List<int> roleIds = Sys_RoleService
                 .Instance
                 .GetAllChildrenRoleId(UserContext.Current.RoleIds);
                return queryable.Where(x => roleIds.Contains(x.Role_Id ?? 0) || x.User_Id == UserContext.Current.UserId);
            };

            base.ExportOnExecuting = (List<Sys_User> list, List<string> ignoreColumn) =>
            {
                if (!ignoreColumn.Contains("Role_Id"))
                {
                    ignoreColumn.Add("Role_Id");
                }
                if (!ignoreColumn.Contains("RoleName"))
                {
                    ignoreColumn.Remove("RoleName");
                }
                WebResponseContent responseData = new WebResponseContent(true);
                return responseData;
            };
            return base.Export(pageData);
        }
    }
}

