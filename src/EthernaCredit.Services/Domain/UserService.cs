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
        public Task<User> FindUserAsync(ClaimsPrincipal userClaims) =>
            FindUserAsync(userClaims.GetEtherAddress(), userClaims.GetEtherPrevAddresses());

        public async Task<User> FindUserAsync(string etherAddress, IEnumerable<string> prevEtherAddresses)
        {
            // Search user.
            var user = await dbContext.Users.QueryElementsAsync(elements =>
                elements.Where(u => u.Address == etherAddress ||
                                    prevEtherAddresses.Contains(u.Address))
                        .FirstOrDefaultAsync());

            // If user doesn't exist.
            if (user is null)
            {
                // Create a new user.
                user = new User(etherAddress);
                await dbContext.Users.CreateAsync(user);

                // Get again, because of https://etherna.atlassian.net/browse/MODM-83
                user = await dbContext.Users.FindOneAsync(u => u.Address == etherAddress);
            }
            else //if already exist
            {
                // Verify if current address is updated, or update it.
                if (user.Address != etherAddress)
                {
                    user.UpdateAddress(etherAddress);
                    await dbContext.SaveChangesAsync();
                }
            }

            return user;
        }
    }
}
