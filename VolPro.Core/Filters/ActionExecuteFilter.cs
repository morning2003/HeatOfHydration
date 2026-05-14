using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using VolPro.Core.Enums;
using VolPro.Core.Extensions;
using VolPro.Core.ObjectActionValidator;
using VolPro.Core.Services;
using VolPro.Core.Utilities;

namespace VolPro.Core.Filters
{
    public class ActionExecuteFilter : IActionFilter
    {

        public void OnActionExecuting(ActionExecutingContext context)
        {
            //验证方法参数
            context.ActionParamsValidator();
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}