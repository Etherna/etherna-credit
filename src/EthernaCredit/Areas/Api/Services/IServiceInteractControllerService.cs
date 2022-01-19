using Etherna.CreditSystem.Areas.Api.DtoModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Api.Services
{
    public interface IServiceInteractControllerService
    {
        Task<CreditDto> GetUserCreditAsync(string address);

        Task<IEnumerable<OperationLogDto>> GetServiceOpLogsWithUserAsync(
            string clientId,
            string address,
            int page,
            int take);

        Task RegisterBalanceUpdateAsync(
            string clientId,
            string address,
            decimal amount,
            string reason);
    }
}