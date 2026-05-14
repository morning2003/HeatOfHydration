using VolPro.Core.Configuration;
using VolPro.Core.Extensions;
using VolPro.Entity.SystemModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VolPro.Core.Controllers.DynamicController
{
    public class ChangeActionService : IHostedService
    {
        private readonly ApplicationPartManager Part;
        private readonly IWebHostEnvironment _environment;
        public ChangeActionService(IServiceScopeFactory scope, IWebHostEnvironment env)
        {
            Part = scope.CreateScope().ServiceProvider.GetService<ApplicationPartManager>();
            _environment = env;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //string rootPath = (_environment.ContentRootPath + "\\plugs").ReplacePath();
            //foreach (var item in Directory.GetFiles(rootPath).Where(x => x.EndsWith(".dll")))
            //{
            //    string path = ($"{item}").ReplacePath();
            //    //assemblyList.Add(Assembly.LoadFile(path));
            //    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            //    var assemblyPart = new AssemblyPart(assembly);
            //    Part.ApplicationParts.Add(assemblyPart);
              
            //}

            //AddControllerChangeProvider.Instance.HasChanged = true;
            //AddControllerChangeProvider.Instance.TokenSource?.Cancel();
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
