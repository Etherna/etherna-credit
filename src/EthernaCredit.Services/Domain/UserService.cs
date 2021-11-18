using Etherna.Authentication.Extensions;
using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.UserAgg;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Services.Domain
{
    class UserService : IUserService
    {
        // Fields.
        private readonly ICreditDbContextInternal dbContext;

        // Constructor.
        public UserService(ICreditDbContext dbContext)
        {
            this.dbContext = (ICreditDbContextInternal)dbContext;
        }

        // Methods.
        public Task<User> FindAndUpdateUserAsync(ClaimsPrincipal user) =>
            FindAndUpdateUserAsync(user.GetEtherAddress(), user.GetEtherPrevAddresses());

        public async Task<User> FindAndUpdateUserAsync(string etherAddress, IEnumerable<string> prevEtherAddresses)
        {
            // Search user.
            var user = await dbContext.Users.QueryElementsAsync(elements =>
                elements.Where(u => u.EtherAddress == etherAddress ||                   //case: service and invoker are synced
                                    prevEtherAddresses.Contains(u.EtherAddress) ||      //case: invoker is ahead than service (update db)
                                    u.EtherPreviousAddresses.Contains(etherAddress))    //case: service is ahead than invoker (do nothing)
                        .FirstOrDefaultAsync());

            // If user doesn't exist.
            if (user is null)
            {
                // Create a new user.
                user = new User(etherAddress);
                await dbContext.Users.CreateAsync(user);

                // Create balance record.
                var balance = new UserBalance(user);
                await dbContext.UserBalances.CreateAsync(balance);

                // Get again, because of https://etherna.atlassian.net/browse/MODM-83
                user = await dbContext.Users.FindOneAsync(user.Id);
            }
            else //if already exist
            {
                // Verify if invoker is ahead of service. In case, update on db.
                if (prevEtherAddresses.Contains(user.EtherAddress))
                {
                    user.UpdateAddresses(etherAddress, prevEtherAddresses);
                    await dbContext.SaveChangesAsync();
                }
            }

            return user;
        }

        public async Task<User> FindUserByAddressAsync(string address) =>
            await dbContext.Users.QueryElementsAsync(elements =>
                elements.Where(u => u.EtherAddress == address ||
                                    u.EtherPreviousAddresses.Contains(address))
                        .FirstAsync());

        public async Task<double> GetUserBalanceAsync(string address)
        {
            var user = await TryFindUserByAddressAsync(address);
            if (user is null)
                return 0;

            return await GetUserBalanceAsync(user);
        }

        public async Task<double> GetUserBalanceAsync(ClaimsPrincipal user)
        {
            var userModel = await FindAndUpdateUserAsync(user);
            return await GetUserBalanceAsync(userModel);
        }

        public async Task<double> GetUserBalanceAsync(User user)
        {
            var userBalance = await dbContext.UserBalances.FindOneAsync(balance => balance.User.Id == user.Id);
            return userBalance.Credit;
        }

        public async Task<bool> IncrementUserBalanceAsync(User user, double ammount, bool allowBalanceDecreaseNegative)
        {
            if (allowBalanceDecreaseNegative || ammount >= 0)
            {
                var balanceResult = await dbContext.UserBalances.Collection.FindOneAndUpdateAsync(
                    balance => balance.User.Id == user.Id,
                    Builders<UserBalance>.Update.Inc(balance => balance.Credit, ammount));

                return balanceResult is not null;
            }
            else
            {
                var balanceResult = await dbContext.UserBalances.Collection.FindOneAndUpdateAsync(
                    balance => balance.User.Id == user.Id &&
                               balance.Credit >= -ammount, //verify disponibility
                    Builders<UserBalance>.Update.Inc(balance => balance.Credit, ammount));

                return balanceResult is not null;
            }
        }

        public async Task<User?> TryFindUserByAddressAsync(string address) =>
            await dbContext.Users.QueryElementsAsync(elements =>
                elements.Where(u => u.EtherAddress == address ||
                                    u.EtherPreviousAddresses.Contains(address))
                        .FirstOrDefaultAsync());
    }
}
