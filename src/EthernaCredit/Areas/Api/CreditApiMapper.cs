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

using Etherna.BeeNet.Models;
using Etherna.Credit.Areas.Api.DtoModels;
using Etherna.Credit.Configs;
using Etherna.Credit.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Etherna.Credit.Areas.Api
{
    public static class CreditApiMapper
    {
        // Methods.
        public static void MapCreditApi(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            // APIs.
            ConfigureV03Maps(app.MapGroup("/api/v0.3").WithMetadata(new CreditApiMarker()));
        }

        // Helpers.
        private static void ConfigureV03Maps(RouteGroupBuilder builder)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            //payments
            builder.MapGet("payments/cryptos",
                    (ICreditApiHandler handler) =>
                        handler.GetAvailablePaymentCryptosAsync())
                .Produces<IEnumerable<PaymentCryptoDto>>();

            //serviceInteract
            builder.MapGet("serviceInteract/users/{address}/credit",
                    (ICreditApiHandler handler,
                            [FromRoute] EthAddress address) =>
                        handler.GetUserCreditAsync(address))
                .RequireAuthorization(CommonConsts.ServiceInteractApiScopePolicy)
                .Produces<CreditDto>()
                .Produces(StatusCodes.Status400BadRequest);
            
            builder.MapGet("serviceInteract/users/{address}/oplogs",
                    (ICreditApiHandler handler,
                            [FromRoute] EthAddress address,
                            [FromQuery] DateTime? fromDate,
                            [FromQuery] DateTime? toDate) =>
                        handler.GetServiceOpLogsWithUserAsync(address, fromDate, toDate))
                .RequireAuthorization(CommonConsts.ServiceInteractApiScopePolicy)
                .Produces<IEnumerable<OperationLogDto>>()
                .Produces(StatusCodes.Status400BadRequest);
            
            builder.MapPut("serviceInteract/users/{address}/credit/balance",
                    (ICreditApiHandler handler,
                            [FromRoute] EthAddress address,
                            [FromQuery] XDaiValue amount,
                            [FromQuery] string reason,
                            [FromQuery] bool isApplied = true) =>
                        handler.RegisterBalanceUpdateAsync(address, amount, isApplied, reason))
                .RequireAuthorization(CommonConsts.ServiceInteractApiScopePolicy)
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest);
            
            //user
            builder.MapGet("user/address",
                    (ICreditApiHandler handler) =>
                        handler.GetCurrentUserAddressAsync())
                .RequireAuthorization(CommonConsts.UserInteractApiScopePolicy)
                .Produces<EthAddress>()
                .IsDeprecated("Get from claims instead");

            builder.MapGet("user/credit",
                    (ICreditApiHandler handler) =>
                        handler.GetCurrentUserCreditAsync())
                .RequireAuthorization(CommonConsts.UserInteractApiScopePolicy)
                .Produces<CreditDto>();

            builder.MapGet("user/logs",
                    (ICreditApiHandler handler,
                            [FromQuery][Range(0, int.MaxValue)] int page = 0,
                            [FromQuery][Range(1, 1000)] int take = 50) =>
                        handler.GetCurrentUserLogsAsync(page, take))
                .RequireAuthorization(CommonConsts.UserInteractApiScopePolicy)
                .Produces<IEnumerable<OperationLogDto>>();
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}