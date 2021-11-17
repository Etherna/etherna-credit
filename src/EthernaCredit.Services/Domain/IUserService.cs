using Etherna.CreditSystem.Domain.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Services.Domain
{
    public interface IUserService
    {
        Task<User> FindAndUpdateUserAsync(ClaimsPrincipal userClaims);
        Task<User> FindAndUpdateUserAsync(string etherAddress, IEnumerable<string> prevEtherAddresses);
        Task<User> FindUserByAddressAsync(string address);
        Task<User?> TryFindUserByAddressAsync(string address);
    }
}