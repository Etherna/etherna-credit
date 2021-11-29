using Etherna.CreditSystem.Areas.Api.DtoModels;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Api.Services
{
    public interface IServiceInteractControllerService
    {
        Task<CreditDto> GetUserCreditAsync(string address);
        Task RegisterBalanceUpdateAsync(string clientId, string address, decimal amount, string reason);
    }
}