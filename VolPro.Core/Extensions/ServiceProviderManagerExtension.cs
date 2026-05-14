using VolPro.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using VolPro.Core.Utilities;

namespace VolPro.Core.Extensions
{
    public static class ServiceProviderManagerExtension
    {
        private static IServiceProvider _serviceProvider;

        public static void UseCache(this IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public static object GetService(this Type serviceType, bool scope = false)
        {
            if (HttpContext.Current == null || scope)
            {
                using (IServiceScope serviceScope = _serviceProvider.CreateScope())
                {
                    return serviceScope.ServiceProvider.GetService(serviceType);
                }
            }
            return HttpContext.Current.RequestServices.GetRequiredService(serviceType);
        }

    }
}
