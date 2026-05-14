using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core;
using VolPro.Core.CacheManager;
using VolPro.Core.Configuration;
using VolPro.Core.Controllers.Basic;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.Extensions;
using VolPro.Core.ManageUser;
using VolPro.Core.Utilities;
using VolPro.Entity.DomainModels;
using VolPro.Sys.IRepositories;
using VolPro.Sys.IServices;

namespace VolPro.WebApi.Controllers.Auth
{
    [Route("api/auth")]
    [Route("api/dataview/auth")]

    public class AuthController : VolController
    {
        private ICacheService _cache;
        private ISys_UserRepository _userRepository;
        private readonly ISys_MenuService _menuService;
        public AuthController(ICacheService cache, ISys_UserRepository userRepository, ISys_MenuService menuService)
        {
            _cache = cache;
            _menuService = menuService;
            _userRepository = userRepository;
        }


        /// <summary>
        /// 获取token
        /// </summary>
        /// <param name="parmas"></param>
        /// <returns></returns>
        [HttpPost, Route("getAccessToken")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult GetAccessToken(string parmas)
        {
            var token = JwtHelper.IssueJwt(new UserInfo { User_Id = UserContext.Current.UserId }, 5);
            token = token.EncryptDES(AppSetting.Secret.JWT);
            return Json(new { token });
        }

        private new IActionResult Error(string message)
        {
            return Json(new { status = false, message });
        }
        /// <summary>
        /// 通过token登录
        /// </summary>
        /// <param name="parmas"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("validationToken"), AllowAnonymous]
        public async Task<IActionResult> ValidationToken([FromBody] AccessInfo access)
        {
            if (access == null || string.IsNullOrEmpty(access.Token))
            {
                return Error("token无效".Translator());
            }
            try
            {
                string token = access.Token.DecryptDES(AppSetting.Secret.JWT);

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSetting.Secret.JWT)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                try
                {
                    var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                    var userInfo = new UserInfo() { User_Id = principal.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Jti).Select(s => s.Value).FirstOrDefault().GetInt() };
                    if (userInfo == null || userInfo.User_Id <= 0)
                    {
                        return Error("token无效或用户信息无效");
                    }
                    var user = await _userRepository.FindAsIQueryable(x => x.User_Id == userInfo.User_Id).FirstOrDefaultAsync();
                    if (user == null || user.User_Id <= 0)
                    {
                        return Error("token无效或用户信息无效");
                    }
                    int expir = AppSetting.ExpMinutes;
                    string accessToken = null;
                    if (AppSetting.FileAuth)
                    {
                        expir = expir + 30;
                        string dt = DateTime.Now.AddMinutes(expir).ToString("yyyy-MM-dd HH:mm");
                        accessToken = $"{user.User_Id}_{dt}".EncryptDES(AppSetting.Secret.User);
                        _cache.Add(accessToken, dt, expir);
                    }
                    token = JwtHelper.IssueJwt(new UserInfo()
                    {
                        User_Id = user.User_Id,
                        UserName = user.UserName,
                        Role_Id = user.Role_Id ?? 0
                    });
                    var data = new { status = true, token, userName = user.UserTrueName, img = user.HeadImageUrl };
                    return Json(data);
                }
                catch (SecurityTokenInvalidSignatureException)
                {
                    return Error("JWT 签名无效，可能被篡改");
                }
                catch (SecurityTokenExpiredException)
                {
                    return Error("JWT 已过期");
                }
                catch (SecurityTokenException)
                {
                    return Error("JWT 校验失败");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析token异常:{ex.Message + ex.StackTrace}");
                return Error("token无效".Translator());
            }

        }

        [HttpPost, Route("getDataViewAccessToken")]
        public IActionResult GetDataViewAccessToken(string parmas)
        {
            int userId = UserContext.Current.UserId;
            string guid = Guid.NewGuid().ToString();
            _cache.Add(guid, userId.ToString(), 180);
            return Content(guid);
        }

        [HttpPost, Route("getDataViewLoginToken"), AllowAnonymous]
        public async Task<IActionResult> GetDataViewLoginToken(string key)
        {
            string value = _cache.Get(key ?? "");
            if (string.IsNullOrEmpty(value))
            {
                return Json(new
                {
                    stataus = false,
                    msg = "key无效"
                });
            }
            // _cache.Remove(key);
            int userId = value.GetInt();
            var user = await _userRepository.FindAsIQueryable(x => x.User_Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                return Json(new
                {
                    stataus = false,
                    msg = "未找到用户信息"
                });
            }
            string token = JwtHelper.IssueJwt(new UserInfo()
            {
                User_Id = user.User_Id,
                UserName = user.UserName,
                Role_Id = user.Role_Id ?? 0
            }, 43200);

            var menu = _menuService.GetUserMenuList(UserContext.Current.GetUserInfo(user.User_Id).RoleIds)
                .Where(x => x.LinkType == 4 && (x.Enable == null || x.Enable == 1))
                 .Select(s => new { s.MenuName, s.Url }).ToList();
            ////_userRepository.Update(user, x => x.Token, true);
            //UserContext.Current.LogOut(user.User_Id);
            var service = UserContext.Current.UserDbService.Select(s => new { id = s.DbServiceId, name = s.DbServiceName })
         .ToList();
            return Json(new
            {
                msg = "操作成功",
                status = true,
                code = 200,
                data = new
                {
                    userinfo = new
                    {
                        id = "0",
                        username = user.UserTrueName,// "admin",
                        nickname = user.UserTrueName
                    },
                    menu,
                    service,
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
                    }
                }
            });
        }
    }
    public class AccessInfo
    {
        public string Token { get; set; }
    }
}
