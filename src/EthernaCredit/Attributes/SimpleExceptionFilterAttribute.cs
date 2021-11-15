using Etherna.MongODM.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;

namespace Etherna.CreditSystem.Attributes
{
    public sealed class SimpleExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            switch (context.Exception)
            {
                case ArgumentException _:
                case FormatException _:
                case InvalidOperationException _:
                case MongodmInvalidEntityTypeException _:
                    context.Result = new BadRequestObjectResult(context.Exception.Message);
                    break;
                case KeyNotFoundException _:
                case MongodmEntityNotFoundException _:
                    context.Result = new NotFoundObjectResult(context.Exception.Message);
                    break;
                case UnauthorizedAccessException _:
                    context.Result = new UnauthorizedResult();
                    break;
            }
        }
    }
}
