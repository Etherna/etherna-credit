using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.UserAgg;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Nethereum.Util;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Services.Domain
{
    class UserService : IUserService
    {
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
                var balance = new UserBalance(user);
                await creditDbContext.UserBalances.CreateAsync(balance);

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

        public async Task<decimal> GetUserBalanceAsync(string address)
        {
            var (user, _) = await TryFindUserAsync(address);
            if (user is null)
                return 0;

            return await GetUserBalanceAsync(user);
        }

        public async Task<decimal> GetUserBalanceAsync(User user)
        {
            var userBalance = await creditDbContext.UserBalances.FindOneAsync(balance => balance.User.Id == user.Id);
            return Decimal128.ToDecimal(userBalance.Credit);
        }

        public async Task<bool> IncrementUserBalanceAsync(User user, decimal amount, bool allowBalanceDecreaseNegative)
        {
            if (user.HasUnlimitedCredit)
                return true;

            if (allowBalanceDecreaseNegative || amount >= 0)
            {
                var balanceResult = await creditDbContext.UserBalances.AccessToCollectionAsync(collection =>
                    collection.FindOneAndUpdateAsync(
                        balance => balance.User.Id == user.Id,
                        Builders<UserBalance>.Update.Inc(balance => balance.Credit, new Decimal128(amount))));

                return balanceResult is not null;
            }
            else
            {
                var balanceResult = await creditDbContext.UserBalances.AccessToCollectionAsync(collection =>
                    collection.FindOneAndUpdateAsync(
                        balance => balance.User.Id == user.Id &&
                                   balance.Credit >= new Decimal128(-amount), //verify disponibility
                        Builders<UserBalance>.Update.Inc(balance => balance.Credit, new Decimal128(amount))));

                return balanceResult is not null;
            }
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
    }
}
