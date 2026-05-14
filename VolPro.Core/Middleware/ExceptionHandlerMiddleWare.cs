using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.Configuration;
using VolPro.Core.Const;
using VolPro.Core.EFDbContext;
using VolPro.Core.Enums;
using VolPro.Core.Extensions;
using VolPro.Core.ManageUser;
using VolPro.Core.Services;

namespace VolPro.Core.Middleware
{
    public class ExceptionHandlerMiddleWare
    {
        private readonly RequestDelegate next;
        public ExceptionHandlerMiddleWare(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                context.Request.EnableBuffering();
                (context.RequestServices.GetService(typeof(ActionObserver)) as ActionObserver).RequestDate = DateTime.Now;
                //文件授权
                if (AppSetting.FileAuth
                    && context.Request.Path.StartsWithSegments("/upload", StringComparison.OrdinalIgnoreCase)
                    && !context.Request.Path.StartsWithSegments("/upload/tables/sys_user", StringComparison.OrdinalIgnoreCase))
                {
                    string key = context.Request.Query["access_token"];
                    bool authResult = false;
                    if (!string.IsNullOrEmpty(key))
                    {
                        //先从缓存读怪
                        string value = context.GetService<Core.CacheManager.ICacheService>().Get(key);
                        if (!string.IsNullOrEmpty(value) && value.GetDateTime() > DateTime.Now)
                        {
                            authResult = true;
                        }
                        else
                        {
                            try
                            {   //缓存丢失直接从解密
                                var dt = key.DecryptDES(AppSetting.Secret.User).Split("_")[1].GetDateTime();
                                authResult = dt > DateTime.Now;
                                if (authResult)
                                {
                                    context.GetService<Core.CacheManager.ICacheService>().Add(key, dt.ToString("yyyy-MM-dd HH:mm"));
                                }
                            }
                            catch { }
                        }
                    }
                    if (!authResult)
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = ApplicationContentType.JSON;
                        await context.Response.WriteAsync(new { message = "Unauthorized", status = false }.Serialize(), Encoding.UTF8);
                        return;
                    }
                }


                await next(context);

                //app.UseMiddleware<ExceptionHandlerMiddleWare>()放在  app.UseRouting()后才可以在await next(context);前执行
                Endpoint endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
                if (endpoint != null && endpoint is RouteEndpoint routeEndpoint)
                {
                    ActionLog log = endpoint.Metadata.GetMetadata<ActionLog>();
                    if (log != null && log.Write)
                    {
                        Logger.Add(log?.LogType, null, null, null, status: LoggerStatus.Info);
                    }
                }
                else
                {
                    Logger.Info(LoggerType.Info);
                }
            }
            catch (Exception exception)
            {
                var env = context.RequestServices.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
                string message = exception.Message + exception.InnerException;
                Logger.Error(LoggerType.Exception, message);
                if (!env.IsDevelopment())
                {
                    message = "服务器处理异常".Translator();
                }
                else
                {
                    Console.WriteLine($"服务器处理出现异常:{message}");
                }
                context.Response.StatusCode = 500;
                context.Response.ContentType = ApplicationContentType.JSON;
                await context.Response.WriteAsync(new { message, status = false }.Serialize(), Encoding.UTF8);
            }
        }
    }
}
