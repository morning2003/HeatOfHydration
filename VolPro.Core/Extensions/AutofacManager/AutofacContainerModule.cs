using VolPro.Core.Extensions;
using System;
using VolPro.Core.Configuration;

namespace VolPro.Core.Extensions.AutofacManager
{
    public class AutofacContainerModule
    {
        public static TService GetService<TService>() where TService:class
        {
            return typeof(TService).GetService() as TService;
        }
    }
}
