using Etherna.CreditSystem.Domain.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Services.Domain
{
    public interface IUserService
    {
        Task<User> FindUserAsync(ClaimsPrincipal userClaims);
        Task<User> FindUserAsync(string etherAddress, IEnumerable<string> prevEtherAddresses);
    }
}