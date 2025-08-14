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
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.Credit.Areas.Api.Services
{
    internal sealed class ServiceInteractControllerService(
        ICreditDbContext dbContext,
        IEthernaOpenIdConnectClient ethernaOidcClient,
        IUserService userService)
        : IServiceInteractControllerService
    {
        // Methods.
        public async Task<IEnumerable<OperationLogDto>> GetServiceOpLogsWithUserAsync(
            string address,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var clientId = await ethernaOidcClient.GetClientIdAsync();

            var (user, userSharedInfo) = await userService.TryFindUserAsync(address);
            if (user is null)
                return [];

            var result = await dbContext.OperationLogs.QueryElementsAsync(
                elements => elements.Where(l => l.Author == clientId)
                                    .Where(l => l.User.Id == user.Id)
                                    .Where(l => l.CreationDateTime >= (fromDate ?? DateTime.MinValue))
                                    .Where(l => l.CreationDateTime <= (toDate ?? DateTime.MaxValue))
                                    .OrderBy(l => l.CreationDateTime)
                                    .ToListAsync());

            return result.Select(l => new OperationLogDto(l, userSharedInfo!));
        }

        public async Task<CreditDto> GetUserCreditAsync(string address)
        {
            var (user, _) = await userService.TryFindUserAsync(address);
            if (user is null)
                return new CreditDto(0, false);

            var balance = await userService.GetUserBalanceAsync(user);

            return new CreditDto(balance, user.HasUnlimitedCredit);
        }

        public async Task RegisterBalanceUpdateAsync(
            string address,
            XDaiValue amount,
            bool isApplied,
            string reason)
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
        }
    }
}
