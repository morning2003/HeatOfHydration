using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VolPro.Core.Controllers.DynamicController
{
    public class AddControllerChangeProvider : IActionDescriptorChangeProvider
    {
        public static AddControllerChangeProvider Instance { get; } = new AddControllerChangeProvider();
        public CancellationTokenSource TokenSource { get; private set; }
        public bool HasChanged { get; set; }
        public IChangeToken GetChangeToken()
        {
            TokenSource = new CancellationTokenSource();
            return new CancellationChangeToken(TokenSource.Token);
        }
    }
}
