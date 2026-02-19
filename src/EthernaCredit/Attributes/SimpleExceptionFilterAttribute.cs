// Copyright 2021-present Etherna SA
// This file is part of Etherna Credit.
// 
// Etherna Credit is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Credit is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Credit.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.MongODM.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System;
using System.Collections.Generic;

namespace Etherna.Credit.Attributes
{
    internal sealed class SimpleExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            
            // Log exception.
            Log.Warning(context.Exception, "API exception");

            switch (context.Exception)
            {
                // Error code 400.
                case ArgumentException _:
                case FormatException _:
                case MongodmInvalidEntityTypeException _:
                    context.Result = new BadRequestObjectResult(context.Exception.Message);
                    break;
                
                // Error code 401.
                case UnauthorizedAccessException _:
                    context.Result = new UnauthorizedResult();
                    break;
                
                // Error code 404.
                case KeyNotFoundException _:
                case MongodmEntityNotFoundException _:
                    context.Result = new NotFoundObjectResult(context.Exception.Message);
                    break;
                
                // Error code 500.
                case InvalidOperationException _:
                default:
                    context.Result = new StatusCodeResult(500);
                    break;
            }
        }
    }
}
