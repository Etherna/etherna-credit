using Etherna.CreditSystem.Domain.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Services.Domain
{
    public interface IUserService
    {
        Task<User> FindAndUpdateUserAsync(ClaimsPrincipal user);
        Task<User> FindAndUpdateUserAsync(string etherAddress, IEnumerable<string> prevEtherAddresses);
        Task<User> FindUserByAddressAsync(string address);
        Task<double> GetUserBalanceAsync(string address);
        Task<double> GetUserBalanceAsync(ClaimsPrincipal user);
        Task<double> GetUserBalanceAsync(User user);
        Task<bool> IncrementUserBalanceAsync(User user, double ammount, bool allowBalanceDecreaseNegative);
        Task<User?> TryFindUserByAddressAsync(string address);
    }
}