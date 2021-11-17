using Etherna.Authentication.Extensions;
using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models;
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
        private readonly ICreditDbContext dbContext;

        // Constructor.
        public UserService(ICreditDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // Methods.
        public Task<User> FindAndUpdateUserAsync(ClaimsPrincipal userClaims) =>
            FindAndUpdateUserAsync(userClaims.GetEtherAddress(), userClaims.GetEtherPrevAddresses());

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

                // Get again, because of https://etherna.atlassian.net/browse/MODM-83
                user = await dbContext.Users.FindOneAsync(u => u.EtherAddress == etherAddress);
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

        public async Task<User?> TryFindUserByAddressAsync(string address) =>
            await dbContext.Users.QueryElementsAsync(elements =>
                elements.Where(u => u.EtherAddress == address ||
                                    u.EtherPreviousAddresses.Contains(address))
                        .FirstOrDefaultAsync());
    }
}
