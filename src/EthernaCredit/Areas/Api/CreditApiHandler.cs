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

using Etherna.Authentication;
using Etherna.BeeNet.Models;
using Etherna.Credit.Areas.Api.DtoModels;
using Etherna.Credit.Domain;
using Etherna.Credit.Domain.Models;
using Etherna.Credit.Domain.Models.OperationLogs;
using Etherna.Credit.Services.Domain;
using Etherna.Credit.Shkeeper;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.Credit.Areas.Api
{
    internal sealed class CreditApiHandler(
        ICreditDbContext dbContext,
        IEthernaOpenIdConnectClient ethernaOidcClient,
        IShKeeperService shkeperService,
        IUserService userService)
        : ICreditApiHandler
    {
        public Task<IResult> CreateCryptoPaymentRequestAsync(XDaiValue amount, string cryptoSymbol) =>
            ExceptionHandler.RunAsync(async () =>
            {
                // Verify auth and input.
                var (author, _) = await userService.FindUserAsync(await ethernaOidcClient.GetEtherAddressAsync());
                var availableCryptos = await shkeperService.GetAvailableCryptosAsync();
                if (!availableCryptos.ContainsKey(cryptoSymbol))
                    return Results.BadRequest($"Crypto symbol {cryptoSymbol} not found");
                
                // Create payment request on db.
                var request = new CryptoPaymentRequest(author, amount, cryptoSymbol);
                await dbContext.CryptoPaymentRequests.CreateAsync(request);
                
                // Create payment request on ShKeeper.
                var shkeeperResponse = await shkeperService.CreatePaymentRequestAsync(
                    amount,
                    cryptoSymbol,
                    request.Id);

                return Results.Json(new CryptoPaymentRequestDto(
                    request.Id,
                    amount,
                    shkeeperResponse.CryptoAmount,
                    shkeeperResponse.DisplayName,
                    cryptoSymbol,
                    shkeeperResponse.ExchangeRate,
                    shkeeperResponse.RecalculateAfter,
                    shkeeperResponse.Status,
                    shkeeperResponse.Wallet));
            });

        public Task<IResult> GetAvailablePaymentCryptosAsync() =>
            ExceptionHandler.RunAsync(async () =>
            {
                var cryptos = await shkeperService.GetAvailableCryptosAsync();
                return Results.Json(cryptos.Values.Select(c => new PaymentCryptoDto(c.DisplayName, c.Symbol)));
            });

        public Task<IResult> GetCurrentUserAddressAsync() =>
            ExceptionHandler.RunAsync(async () =>
            {
                EthAddress address = await ethernaOidcClient.GetEtherAddressAsync();
                return Results.Json(address);
            });

        public Task<IResult> GetCurrentUserCreditAsync() =>
            ExceptionHandler.RunAsync(async () =>
            {
                var (userModel, _) = await userService.FindUserAsync(await ethernaOidcClient.GetEtherAddressAsync());
                var balance = await userService.GetUserBalanceAsync(userModel);
                return Results.Json(new CreditDto(balance, userModel.HasUnlimitedCredit));
            });

        public Task<IResult> GetCurrentUserLogsAsync(int page, int take) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var (userModel, userSharedInfo) = await userService.FindUserAsync(await ethernaOidcClient.GetEtherAddressAsync());
                var paginatedLogs = await dbContext.OperationLogs.QueryPaginatedElementsAsync(
                    elements => elements.Where(l => l.User.Id == userModel.Id),
                    l => l.CreationDateTime,
                    page,
                    take);
                return Results.Json(paginatedLogs.Elements.Select(l => new OperationLogDto(l, userSharedInfo)));
            });
        
        public Task<IResult> GetServiceOpLogsWithUserAsync(
            EthAddress address,
            DateTime? fromDate,
            DateTime? toDate) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var clientId = await ethernaOidcClient.GetClientIdAsync();

                var (user, userSharedInfo) = await userService.TryFindUserAsync(address);
                if (user is null)
                    return Results.Json(Array.Empty<OperationLogDto>());

                var result = await dbContext.OperationLogs.QueryElementsAsync(
                    elements => elements.Where(l => l.Author == clientId)
                        .Where(l => l.User.Id == user.Id)
                        .Where(l => l.CreationDateTime >= (fromDate ?? DateTime.MinValue))
                        .Where(l => l.CreationDateTime <= (toDate ?? DateTime.MaxValue))
                        .OrderBy(l => l.CreationDateTime)
                        .ToListAsync());

                return Results.Json(result.Select(l => new OperationLogDto(l, userSharedInfo!)));
            });

        public Task<IResult> GetUserCreditAsync(EthAddress address) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var (user, _) = await userService.TryFindUserAsync(address);
                if (user is null)
                    return Results.Json(new CreditDto(0, false));

                var balance = await userService.GetUserBalanceAsync(user);

                return Results.Json(new CreditDto(balance, user.HasUnlimitedCredit));
            });

        public Task<IResult> RegisterBalanceUpdateAsync(
            EthAddress address,
            XDaiValue amount,
            bool isApplied,
            string reason) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var clientId = await ethernaOidcClient.GetClientIdAsync();

                // Get user.
                var (user, _) = await userService.FindUserAsync(address);

                // Apply update (balance can go negative).
                if (isApplied)
                {
                    var result = await userService.TryIncrementUserBalanceAsync(user, amount, true);
                    if (!result)
                        throw new InvalidOperationException();
                }

                // Create or update log.
                var updatedLog = await dbContext.OperationLogs.AccessToCollectionAsync(collection =>
                    collection.FindOneAndUpdateAsync(
                        Builders<OperationLogBase>.Filter.OfType<UpdateOperationLog>(
                            log => log.Author == clientId &&
                                   log.CreationDateTime >= DateTime.Now.Date &&
                                   log.IsApplied == isApplied &&
                                   log.Reason == reason &&
                                   log.User.Id == user.Id),
                        Builders<OperationLogBase>.Update.Inc(log => log.Amount, amount)));

                if (updatedLog is null) //if a previous log didn't exist
                {
                    var withdrawLog = new UpdateOperationLog(amount, clientId, isApplied, reason, user);
                    await dbContext.OperationLogs.CreateAsync(withdrawLog);
                }

                return Results.Ok();
            });
    }
}