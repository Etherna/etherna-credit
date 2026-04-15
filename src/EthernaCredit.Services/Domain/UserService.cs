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
using Etherna.Credit.Domain;
using Etherna.Credit.Domain.Models;
using Etherna.Credit.Domain.Models.OperationLogs;
using Etherna.Credit.Domain.Models.UserAgg;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.Credit.Services.Domain
{
    internal sealed class UserService(
        ICreditDbContext creditDbContext,
        IOptions<UserServiceOptions> options,
        ISharedDbContext sharedDbContext)
        : IUserService
    {
        // Fields.
        private readonly ICreditDbContextInternal creditDbContext = (ICreditDbContextInternal)creditDbContext;
        private readonly UserServiceOptions options = options.Value;

        // Methods.
        public async Task<(User, UserSharedInfo)> FindUserAsync(EthAddress address) =>
            await FindUserAsync(await FindUserSharedInfoByAddressAsync(address));

        public async Task<(User, UserSharedInfo)> FindUserAsync(UserSharedInfo userSharedInfo)
        {
            // Try find user.
            var user = await creditDbContext.Users.TryFindOneAsync(u => u.SharedInfoId == userSharedInfo.Id);

            // If user doesn't exist.
            if (user is null)
            {
                // Create a new user.
                user = new User(userSharedInfo);
                await creditDbContext.Users.CreateAsync(user);

                // Create balance record.
                var welcomeCredit = options.WelcomeCredit;
                var balance = new UserBalance(user, welcomeCredit);
                await creditDbContext.UserBalances.CreateAsync(balance);

                // Create log for welcome credit deposit.
                if (welcomeCredit > 0)
                {
                    var depositLog = new WelcomeCreditDepositOperationLog(
                        welcomeCredit, userSharedInfo.EtherAddress.ToString(), user);
                    await creditDbContext.OperationLogs.CreateAsync(depositLog);
                }

                // Get again, because of https://etherna.atlassian.net/browse/MODM-83
                user = await creditDbContext.Users.FindOneAsync(user.Id);
            }

            return (user, userSharedInfo);
        }

        public async Task<UserSharedInfo> FindUserSharedInfoByAddressAsync(EthAddress address)
        {
            // Find user shared info.
            return await sharedDbContext.UsersInfo.QueryElementsAsync(elements =>
                elements.Where(u => u.EtherAddress == address ||                   //case: db and invoker are synced
                                    u.EtherPreviousAddresses.Contains(address))    //case: db is ahead than invoker
                        .FirstAsync());
        }

        public async Task<XDaiValue> GetUserBalanceAsync(EthAddress address)
        {
            var (user, _) = await TryFindUserAsync(address);
            if (user is null)
                return 0;

            return await GetUserBalanceAsync(user);
        }

        public async Task<XDaiValue> GetUserBalanceAsync(User user)
        {
            var userBalance = await creditDbContext.UserBalances.FindOneAsync(balance => balance.User.Id == user.Id);
            return userBalance.Credit;
        }

        public async Task<(User?, UserSharedInfo?)> TryFindUserAsync(EthAddress address)
        {
            var sharedInfo = await TryFindUserSharedInfoByAddressAsync(address);
            if (sharedInfo is null)
                return (null, null);

            return await FindUserAsync(sharedInfo);
        }

        public async Task<UserSharedInfo?> TryFindUserSharedInfoByAddressAsync(EthAddress address)
        {
            try { return await FindUserSharedInfoByAddressAsync(address); }
            catch (InvalidOperationException) { return null; }
        }

        public async Task<bool> TryIncrementUserBalanceAsync(User user, XDaiValue amount, bool allowBalanceDecreaseNegative)
        {
            if (user.HasUnlimitedCredit)
                return true;

            if (allowBalanceDecreaseNegative || amount >= 0)
            {
                var balanceResult = await creditDbContext.UserBalances.AccessToCollectionAsync(collection =>
                    collection.FindOneAndUpdateAsync(
                        balance => balance.User.Id == user.Id,
                        Builders<UserBalance>.Update.Inc(balance => balance.Credit, amount)));

                return balanceResult is not null;
            }
            else
            {
                var balanceResult = await creditDbContext.UserBalances.AccessToCollectionAsync(collection =>
                    collection.FindOneAndUpdateAsync(
                        balance => balance.User.Id == user.Id &&
                                   balance.Credit >= -amount, //verify disponibility
                        Builders<UserBalance>.Update.Inc(balance => balance.Credit, amount)));

                return balanceResult is not null;
            }
        }
    }
}
