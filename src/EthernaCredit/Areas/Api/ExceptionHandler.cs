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

using Etherna.MongoDB.Driver;
using Etherna.Scrinium.Core.Exceptions;
using Etherna.SwarmSdk.Exceptions;
using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Etherna.Credit.Areas.Api
{
    public static class ExceptionHandler
    {
        [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
        public static async Task<IResult> RunAsync(Func<Task<IResult>> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            
            try
            {
                return await action();
            }
            catch (Exception e)
            {
                // Log exception.
                Log.Warning(e, "API exception");

                switch (e)
                {
                    // Error code 400.
                    case ArgumentException:
                    case FormatException:
                    case ScriniumInvalidEntityTypeException:
                        return ErrorResults.GetBadRequestErrorResult();

                    // Error code 401.
                    case UnauthorizedAccessException:
                        return ErrorResults.GetUnauthorizedErrorResult();

                    // Error code 404.
                    case SwarmSdkApiException { StatusCode: 404 }:
                    case KeyNotFoundException:
                    case ScriniumEntityNotFoundException:
                        return ErrorResults.GetNotFoundErrorResult();

                    // Error code 409.
                    // A transaction that lost a write conflict with a concurrent one, after the retries
                    // of the db context: the request is legitimate and the client can repeat it, so answer
                    // a conflict instead of a server error.
                    case MongoException mongoException when mongoException.HasErrorLabel("TransientTransactionError"):
                        return ErrorResults.GetErrorResult(StatusCodes.Status409Conflict, "Concurrent write conflict, retry the request");
                    
                    // Error code 503.
                    case SwarmSdkApiException:
                        return ErrorResults.GetServiceUnavailableErrorResult();
                        
                    // Error code 500.
                    case InvalidOperationException:
                    default:
                        return ErrorResults.GetInternalServerErrorResult();
                }
            }
        }
    }
}