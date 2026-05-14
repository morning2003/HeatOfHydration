using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;
using VolPro.Core;
using VolPro.Core.Configuration;
using VolPro.Core.Controllers.Basic;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.Enums;
using VolPro.Core.Extensions;
using VolPro.Core.ManageUser;
using VolPro.Core.ObjectActionValidator;
using VolPro.Core.Services;
using VolPro.Core.Utilities;
using VolPro.Entity.DomainModels;
using VolPro.Sys.IRepositories;
using VolPro.Sys.IServices;

namespace VolPro.WebApi.Controllers.DataView
{
    [Route("api/dataview/sys")]

    public class DataViewSysController : VolController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISys_UserRepository _userRepository;
        private readonly ISys_MenuService _menuService;
        private readonly IMemoryCache _memoryCache;
        public DataViewSysController(IHttpContextAccessor httpContextAccessor,
            ISys_UserRepository userRepository,
            ISys_MenuService menuService,
            IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
            _memoryCache = memoryCache;
            _menuService = menuService;
        }
        [Route("login"), HttpPost, AllowAnonymous]
        [ObjectModelValidatorFilter(ValidatorModel.Login)]
        public async Task<IActionResult> Login([FromBody] LoginInfo loginInfo)
        {

            WebResponseContent webResponse = new WebResponseContent();
            string msg = string.Empty;

            string cacheCode = (_memoryCache.Get(loginInfo.UUID) ?? "").ToString();
            if (string.IsNullOrEmpty(cacheCode))
            {
                return Error("验证码已失效".Translator());
            }
            if (cacheCode.ToLower() != loginInfo.VerificationCode.ToLower())
            {
                _memoryCache.Remove(loginInfo.UUID);
                return Error("验证码不正确".Translator());
            }
            try
            {
                var user = await _userRepository.FindAsIQueryable(x => x.UserName == loginInfo.UserName)
                    .FirstOrDefaultAsync();

                if (user == null || loginInfo.Password.Trim().EncryptDES(AppSetting.Secret.User) != (user.UserPwd ?? ""))
                    return Error("账号或密码不正确");

                string token = JwtHelper.IssueJwt(new UserInfo()
                {
                    User_Id = user.User_Id,
                    UserName = user.UserName,
                    Role_Id = user.Role_Id ?? 0
                }, 43200);
                user.Token = token;
                webResponse.Data = new { token, userName = user.UserTrueName, img = user.HeadImageUrl };
                _userRepository.Update(user, x => x.Token, true);
                UserContext.Current.LogOut(user.User_Id);

                loginInfo.Password = string.Empty;

                var menu = _menuService.GetUserMenuList(UserContext.Current.GetUserInfo(user.User_Id).RoleIds)
                      .Where(x => x.LinkType == 4 && (x.Enable == null || x.Enable == 1))
                    .Select(s => new { s.MenuName, s.Url }).ToList();


                //   return webResponse.OK(ResponseType.LoginSuccess);
                return Json(new
                {
                    msg = "操作成功",
                    code = 200,
                    data = new
                    {
                        userinfo = new
                        {
                            id = "0",
                            username = user.UserTrueName,// "admin",
                                                         // password = "21232f297a57a5a743894a0e4a801fc3",
                            nickname = user.UserTrueName,
                            DefaultUrl = user.DefaultUrl,
                        },
                        token = new
                        {
                            tokenName = "Authorization",
                            tokenValue = $"Bearer {token}",
                            isLogin = true,
                            loginId = "1",
                            loginType = "login",
                            tokenTimeout = 2592000 * 600,
                            sessionTimeout = 2592000 * 600,
                            tokenSessionTimeout = 2591893,
                            tokenActivityTimeout = -1,
                            loginDevice = "default-device"
                        },
                        menu = menu
                    }
                });
            }
            catch (Exception ex)
            {
                msg = ex.Message + ex.StackTrace;
                if (HttpContext.GetService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>().IsDevelopment())
                {
                    throw new Exception(ex.Message + ex.StackTrace);
                }
                return Error(msg);
            }
            finally
            {
                _memoryCache.Remove(loginInfo.UUID);
                Logger.Info(LoggerType.Login, loginInfo.Serialize(), webResponse.Message, msg);
            }


            //return Json(data);
        }
        /// <summary>
        /// 2020.06.15增加登陆验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("getVierificationCode"), AllowAnonymous]
        public IActionResult GetVierificationCode()
        {
            string code = VierificationCode.RandomText();
            var data = new
            {
                img = VierificationCode.CreateBase64Imgage(code),
                uuid = Guid.NewGuid()
            };
            _memoryCache.Set(data.uuid.ToString(), code, new TimeSpan(0, 5, 0));
            return Json(new { code = 200, data = data });
        }
        [Route("logout"), HttpGet, AllowAnonymous]
        public IActionResult LogOut()
        {
            return Json(new { code = 200, msg = "ok" });
        }
        [HttpGet, Route("getOssInfo"), AllowAnonymous]
        public IActionResult GetOssInfo()
        {
            return Json(new
            {
                bucketName = "dataview",
                bucketURL = ""
            });
        }

        private new IActionResult Error(string msg = "error")
        {
            return Json(new
            {
                code = 0,
                msg = msg
            });
        }
    }
}
