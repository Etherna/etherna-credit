using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.UserAgg;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Services.Domain
{
    public interface IUserService
    {
        Task<(User, UserSharedInfo)> FindUserAsync(string address);
        Task<(User, UserSharedInfo)> FindUserAsync(UserSharedInfo userSharedInfo);
        Task<UserSharedInfo> FindUserSharedInfoByAddressAsync(string address);
        Task<decimal> GetUserBalanceAsync(string address);
        Task<decimal> GetUserBalanceAsync(User user);
        Task<bool> IncrementUserBalanceAsync(User user, decimal amount, bool allowBalanceDecreaseNegative);
        Task<(User?, UserSharedInfo?)> TryFindUserAsync(string address);
        Task<UserSharedInfo?> TryFindUserSharedInfoByAddressAsync(string address);
    }
}