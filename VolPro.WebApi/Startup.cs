using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Swashbuckle.AspNetCore.SwaggerGen;
using VolPro.Core.Configuration;
using VolPro.Core.Extensions;
using VolPro.Core.Filters;
using VolPro.Core.Language;
//using VolPro.Core.KafkaManager.IService;
//using VolPro.Core.KafkaManager.Service;
using VolPro.Core.Middleware;
using VolPro.Core.ObjectActionValidator;
using VolPro.Core.Quartz;
using VolPro.Core.Utilities.PDFHelper;
using VolPro.Core.WorkFlow;
using VolPro.Entity.DomainModels;
using VolPro.WebApi.Controllers.Hubs;
using VolPro.Core.Controllers.DynamicController;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using VolPro.Core.Print;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.Controllers.Basic;

namespace VolPro.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private IServiceCollection Services { get; set; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //初始化模型验证配置
            services.UseMethodsModelParameters().UseMethodsGeneralParameters();
            services.AddSingleton<IObjectModelValidator>(new NullObjectModelValidator());
            Services = services;
            // services.Replace( ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());
            services.AddSession();
            services.AddMemoryCache();
            services.AddHttpContextAccessor();
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ApiAuthorizeFilter));
                options.Filters.Add(typeof(ActionExecuteFilter));
                //  options.SuppressAsyncSuffixInActionNames = false;
            });
            services.AddControllers()
              .AddNewtonsoftJson(op =>
              {
                  op.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                  op.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                  op.SerializerSettings.Converters.Add(new LongCovert());
              });

            Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(options =>
             {
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     SaveSigninToken = true,//保存token,后台验证token是否生效(重要)
                     ValidateIssuer = true,//是否验证Issuer
                     ValidateAudience = true,//是否验证Audience
                     ValidateLifetime = true,//是否验证失效时间
                     ValidateIssuerSigningKey = true,//是否验证SecurityKey
                     ValidAudience = AppSetting.Secret.Audience,//Audience
                     ValidIssuer = AppSetting.Secret.Issuer,//Issuer，这两项和前面签发jwt的设置一致
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSetting.Secret.JWT))
                 };
                 options.Events = new JwtBearerEvents()
                 {
                     OnChallenge = context =>
                     {
                         context.HandleResponse();
                         context.Response.Clear();
                         context.Response.ContentType = "application/json";
                         context.Response.StatusCode = 401;
                         context.Response.WriteAsync(new { message = "授权未通过", status = false, code = 401 }.Serialize());
                         return Task.CompletedTask;
                     }
                 };
             });
            //必须appsettings.json中配置
            string corsUrls = Configuration["CorsUrls"];
            if (string.IsNullOrEmpty(corsUrls))
            {
                throw new Exception("请配置跨请求的前端Url");
            }
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                        builder =>
                        {
                            builder.AllowAnyOrigin()
                           .SetPreflightMaxAge(TimeSpan.FromSeconds(2520))
                            .AllowAnyHeader().AllowAnyMethod();
                        });
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                //分为2份接口文档
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "VolPro.Core后台Api", Version = "v1", Description = "这是对文档的描述。。" });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "VolPro.Core对外三方Api", Version = "v2", Description = "xxx接口文档" });  //控制器里使用[ApiExplorerSettings(GroupName = "v2")]              
                                                                                                                                //启用中文注释功能
                                                                                                                                // var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                                                                                                                                //  var xmlPath = Path.Combine(basePath, "VolPro.WebApi.xml");
                                                                                                                                //   c.IncludeXmlComments(xmlPath, true);//显示控制器xml注释内容
                                                                                                                                //添加过滤器 可自定义添加对控制器的注释描述
                                                                                                                                //c.DocumentFilter<SwaggerDocTag>();

                var security = new Dictionary<string, IEnumerable<string>> { { AppSetting.Secret.Issuer, new string[] { } } };
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "JWT授权token前面需要加上字段Bearer与一个空格,如Bearer token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            })
             .AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressConsumesConstraintForFormFileParameters = true;
                options.SuppressInferBindingSourcesForParameters = true;
                options.SuppressModelStateInvalidFilter = true;
                options.SuppressMapClientErrors = true;
                options.ClientErrorMapping[404].Link =
                    "https://*/404";
            });
            services.AddSignalR();
            //services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            //services.AddTransient<IPDFService, PDFService>();
            services.AddHttpClient();
            services.AddScoped<Hncdi.HeatOfHydration.Services.AiAdviceService>();
            Services.AddTransient<HttpResultfulJob>();
            Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            Services.AddSingleton<Quartz.Spi.IJobFactory, IOCJobFactory>();

            //   Services.AddSingleton<IActionDescriptorChangeProvider>(AddControllerChangeProvider.Instance);
            Services.AddHostedService<ChangeActionService>();


            ////设置文件上传大小限制
            ///大文件上传，前端可能直接提示的【服务器异常】，后台调试接口fileInput是null
            ///再调试HttpContext.里面的form值提示Request body too large. The max request body size is 104857600 bytes.
            //services.Configure<FormOptions>(x =>
            //{
            //    x.MultipartBodyLengthLimit = 1024 * 1024 * 100;//100M
            //});
            //services.Configure<KestrelServerOptions>(options =>
            //{
            //    options.Limits.MaxRequestBodySize = 1024 * 1024 * 100;//100M
            //});
            //services.Configure<IISServerOptions>(options =>
            //{
            //    options.MaxRequestBodySize = 1024 * 1024 * 100;//100M
            //});

            //services.AddSession();
            //services.AddMemoryCache();
            //初始化配置文件
            AppSetting.Init(services, Configuration);
            services.UseSqlSugar();
        }
        public void ConfigureContainer(ContainerBuilder builder)
        {
            Services.AddModule(builder, Configuration);
            WorkFlowContainer.Instance
                .Use<Demo_Order, Demo_OrderList>("订单管理",
                    //过滤条件字段
                    filterFields: x => new { x.OrderDate, x.OrderNo, x.OrderStatus, x.OrderType, x.Creator, x.CustomerId },
                    //审批界面显示表数据字段
                    formFields: x => new { x.OrderDate, x.OrderNo, x.OrderStatus, x.Remark },
                    //明细表显示的数据
                    formDetailFields: x => new { x.GoodsName, x.GoodsCode, x.Qty, x.Price, x.Specs, x.Img, x.CreateDate },
                    //defaultAduitStatus: AuditStatus.草稿,
                    //可编辑的字段(字段必须在上面的formFields内)
                    editFields: x => new { x.OrderDate, x.OrderNo, x.OrderStatus, x.OrderType }
                )
                .Use<Demo_Product>(
                    "产品管理",
                    filterFields: x => new { x.Creator, x.Price, x.ProductCode, x.ProductName },
                    formFields: x => new { x.ProductCode, x.ProductName, x.Remark, x.Price, x.Creator, x.CreateDate }
                    )
                .Run();

            /**********************************打印配置****************************************************/
            PrintContainer.Instance
                 /*****************[全国城市]单表打印*****************/
                 .Use<Sys_Region>(
                   //主表配置
                   name: "地址打印管理",
                   //主表可以打印的字段
                   printFields: x => new { x.name, x.code, x.mername, x.pinyin, x.Lat, x.Lng }
                 )
                 //主从表同时打印(注意Use第一个参数是主表，第二个明细表)
                 .Use<Demo_Order, Demo_OrderList>(
                   //主表配置
                   name: "订单打印管理",
                   //主表可以打印的字段
                   printFields: x => new { x.OrderNo, x.OrderStatus, x.OrderType, x.OrderDate, x.TotalPrice, x.TotalQty },

                   //明细表配置
                   detailName: "订单详情",
                   //明细表可以打印的字段
                   detailPrintFields: c => new { c.GoodsCode, c.GoodsName, c.Specs, c.Price, c.Qty }
                 )
                 /*****************[订单表]打印配置(主从表明细表(一对一))*****************/
                 .Use<Demo_Order, Demo_OrderList>(
                   //主表配置
                   name: "订单打印管理",
                   //主表可以打印的字段
                   printFields: x => new { x.OrderNo, x.OrderStatus, x.OrderType, x.OrderDate, x.TotalPrice, x.TotalQty },
                   //主表自定义打印的字段,没有就填null;需要在:PrintCustom类QueryResult字自定义返回这些字段的值
                   customFields: new List<CustomField>() {
                            new CustomField() { Name="自定义1",   Field="test1"  } ,
                            new CustomField() {  Name="自定义2",  Field="test2" }
                    },
                   //明细表配置
                   detail: new PrintDetailOptions<Demo_OrderList>()
                   {
                       Name = "订单详情",
                       PrintFields = c => new { c.GoodsCode, c.GoodsName, c.Img, c.Specs, c.Price, c.Qty },
                       //明细表自定义的字段,没有就填null;需要在:PrintCustom类QueryResult字自定义返回这些字段的值
                       CustomFields = new List<CustomField>() {
                            new CustomField() { Name="明细自定义1",   Field="test1"  } ,
                            new CustomField() { Name="明细自定义2",  Field="test2" }
                       }
                   }
                 )
                 /*****************[产品表]打印配置(一对多打印)*****************/
                 .Use<Demo_Product, Demo_ProductSize, Demo_ProductColor>(
                       //主表配置
                       name: "一对多打印测试",
                       //主表可以打印的字段
                       printFields: x => new { x.ProductName, x.ProductCode, x.AuditStatus, x.Price, x.Remark, x.CreateDate },
                       //主表自定义打印的字段,没有就填null;需要在:PrintCustom类QueryResult字自定义返回这些字段的值
                       customFields: null,//配置同上
                                          //明细表[产品尺寸]打印配置
                       detail1: new PrintDetailOptions<Demo_ProductSize>()
                       {
                           Name = "产品尺寸",
                           //明细表打印的字段
                           PrintFields = x => new { x.ProductId, x.Size, x.Unit, x.Remark, x.Creator, x.CreateDate },
                           //明细表自定义的字段,需要在:PrintCustom类QueryResult字自定义返回这些字段的值
                           CustomFields = null //自定义字段同上配置一样
                       },
                       //明细表[产品颜色]打印配置
                       detail2: new PrintDetailOptions<Demo_ProductColor>()
                       {
                           Name = "产品颜色",
                           //明细表打印的字段
                           PrintFields = x => new { x.ProductId, x.Img, x.Color, x.Qty, x.Unit, x.Remark, x.Creator, x.CreateDate },
                           //明细表自定义的字段需要在:PrintCustom类QueryResult字自定义返回这些字段的值
                           CustomFields = null //自定义字段同上配置一样
                       }
                );
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseQuartz(env);
            }
            //2024.06.20增加ApplicationBuilder缓存
            app.ApplicationServices.UseCache();
            app.UseLanguagePack().UseMiddleware<LanguageMiddleWare>();
            app.UseMiddleware<ExceptionHandlerMiddleWare>();
            app.UseDefaultFiles();
            app.UseStaticFiles().UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true
            });
            app.Use(HttpRequestMiddleware.Context);

            //2021.06.27增加创建默认upload文件夹
            string _uploadPath = (env.ContentRootPath + "/Upload").ReplacePath();

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), @"Upload")),
                //配置访问虚拟目录时文件夹别名
                RequestPath = "/Upload",
                OnPrepareResponse = (Microsoft.AspNetCore.StaticFiles.StaticFileResponseContext staticFile) =>
                {
                    //可以在此处读取请求的信息进行权限认证
                    //  staticFile.File
                    //  staticFile.Context.Response.StatusCode;
                }
            });
            //配置HttpContext
            app.UseStaticHttpContext();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                //2个下拉框选项  选择对应的文档
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "VolPro.Core后台Api");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "测试第三方Api");
                c.RoutePrefix = "";
            });
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //配置SignalR
                if (AppSetting.UseSignalR)
                {
                    string corsUrls = Configuration["CorsUrls"];

                    endpoints.MapHub<HomePageMessageHub>("/message")
                    .RequireCors(t =>
                    t.WithOrigins(corsUrls.Split(',')).
                    AllowAnyMethod().
                    AllowAnyHeader().
                    AllowCredentials());
                }

            });
        }
    }

    /// <summary>
    /// Swagger注释帮助类
    /// </summary>
    public class SwaggerDocTag : IDocumentFilter
    {
        /// <summary>
        /// 添加附加注释
        /// </summary>
        /// <param name="swaggerDoc"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            //添加对应的控制器描述
            swaggerDoc.Tags = new List<OpenApiTag>
            {
                new OpenApiTag { Name = "Test", Description = "这是描述" },
                //new OpenApiTag { Name = "你的控制器名字，不带Controller", Description = "控制器描述" },
            };
        }
    }
}
