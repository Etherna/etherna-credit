//   Copyright 2021-present Etherna Sa
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.OperationLogs;
using Etherna.CreditSystem.Domain.Models.UserAgg;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Nethereum.Util;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Services.Domain
{
    internal sealed class UserService : IUserService
    {
        // Consts.
        private const decimal DefaultWelcomeCredit = 0.1M;

        // Fields.
        private readonly ICreditDbContextInternal creditDbContext;
        private readonly ISharedDbContext sharedDbContext;

        // Constructor.
        public UserService(
            ICreditDbContext creditDbContext,
            ISharedDbContext sharedDbContext)
        {
            this.creditDbContext = (ICreditDbContextInternal)creditDbContext;
            this.sharedDbContext = sharedDbContext;
        }

        // Methods.
        public async Task<(User, UserSharedInfo)> FindUserAsync(string address) =>
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
                var welcomeCredit = DefaultWelcomeCredit;
                var balance = new UserBalance(user, welcomeCredit);
                await creditDbContext.UserBalances.CreateAsync(balance);

                // Create log for welcome credit deposit.
                if (welcomeCredit > 0)
                {
                    var depositLog = new WelcomeCreditDepositOperationLog(
                        welcomeCredit, userSharedInfo.EtherAddress, user);
                    await creditDbContext.OperationLogs.CreateAsync(depositLog);
                }

                // Get again, because of https://etherna.atlassian.net/browse/MODM-83
                user = await creditDbContext.Users.FindOneAsync(user.Id);
            }

            return (user, userSharedInfo);
        }

        public async Task<UserSharedInfo> FindUserSharedInfoByAddressAsync(string address)
        {
            if (!address.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("The value is not a valid ethereum address", nameof(address));

            // Normalize address.
            address = address.ConvertToEthereumChecksumAddress();

            // Find user shared info.
            return await sharedDbContext.UsersInfo.QueryElementsAsync(elements =>
                elements.Where(u => u.EtherAddress == address ||                   //case: db and invoker are synced
                                    u.EtherPreviousAddresses.Contains(address))    //case: db is ahead than invoker
                        .FirstAsync());
        }

        public async Task<XDaiBalance> GetUserBalanceAsync(string address)
        {
            var (user, _) = await TryFindUserAsync(address);
            if (user is null)
                return 0;

            return await GetUserBalanceAsync(user);
        }

        public async Task<XDaiBalance> GetUserBalanceAsync(User user)
        {
            var userBalance = await creditDbContext.UserBalances.FindOneAsync(balance => balance.User.Id == user.Id);
            return userBalance.Credit;
        }

        public async Task<(User?, UserSharedInfo?)> TryFindUserAsync(string address)
        {
            var sharedInfo = await TryFindUserSharedInfoByAddressAsync(address);
            if (sharedInfo is null)
                return (null, null);

            return await FindUserAsync(sharedInfo);
        }

        public async Task<UserSharedInfo?> TryFindUserSharedInfoByAddressAsync(string address)
        {
            try { return await FindUserSharedInfoByAddressAsync(address); }
            catch (InvalidOperationException) { return null; }
        }

        public async Task<bool> TryIncrementUserBalanceAsync(User user, XDaiBalance amount, bool allowBalanceDecreaseNegative)
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
