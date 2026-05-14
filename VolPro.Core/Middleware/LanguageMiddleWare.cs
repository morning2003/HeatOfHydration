using VolPro.Core.Language;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VolPro.Core.Middleware
{
    public class LanguageMiddleWare
    {
        private readonly RequestDelegate next;
        public LanguageMiddleWare(RequestDelegate next)
        {
            this.next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value.StartsWith("/lang"))
            {
                context.Response.Headers.TryAdd("Access-Control-Allow-Origin", "*");
                context.Response.ContentType = "application/json";
                //application/json
            }
            if (!context.Request.Headers.ContainsKey("lang"))
            {
                context.Request.Headers["lang"] = LangConst.简体中文;
            }
            await next(context);
        }
    }
}
