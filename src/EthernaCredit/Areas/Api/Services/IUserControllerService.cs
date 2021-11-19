using Etherna.CreditSystem.Areas.Api.DtoModels;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Api.Services
{
    public interface IUserControllerService
    {
        Task<CreditDto> GetCreditAsync(ClaimsPrincipal user);
        Task<IEnumerable<LogDto>> GetLogsAsync(ClaimsPrincipal user, int page, int take);
    }
}