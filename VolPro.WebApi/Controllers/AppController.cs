using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VolPro.Core.Configuration;
using VolPro.Core.Filters;
using VolPro.Core.ManageUser;

namespace VolPro.WebApi.Controllers
{
    [Route("api/app")]
    public class AppController : Controller
    {


        [AllowAnonymous]
        [Route("checkLogin"), HttpGet]
        public IActionResult CheckLogin()
        {
            return Content(UserContext.Current.UserId > 0 ? "1" : "0");
        }

        [AllowAnonymous]
        /// <summary>
        /// 安卓检查更新
        /// </summary>
        /// <param name="home"></param>
        /// <returns></returns>
        [Route("getAndroidVersion"), HttpGet]
        public IActionResult GetAndroidVersion(bool home)
        {
            var section = AppSetting.GetSection("android");
            if (section==null)
            {
                return Json(new { });
            }
            var data = new
            {
                status = true,
                version = section["version"],
                url = section["url"],
                desc = section["desc"]
            };
            return Json(data);
        }

        /// <summary>
        /// IOS检查更新
        /// </summary>
        /// <param name="home"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("getIOSVersion"), HttpGet]
        public IActionResult GetIOSVersion(bool home)
        {
            var section = AppSetting.GetSection("ios");
            if (section == null)
            {
                return Json(new { });
            }
            var data = new
            {
                status = true,
                version = section["version"],
                url = section["url"],
                desc = section["desc"]
            };
            return Json(data);
        }
    }
}
