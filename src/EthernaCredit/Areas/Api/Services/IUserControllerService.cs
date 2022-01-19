using Etherna.CreditSystem.Areas.Api.DtoModels;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Api.Services
{
    public interface IUserControllerService
    {
        string GetAddress(ClaimsPrincipal user);
        Task<CreditDto> GetCreditAsync(ClaimsPrincipal user);
        Task<IEnumerable<OperationLogDto>> GetLogsAsync(ClaimsPrincipal user, int page, int take);
    }
}