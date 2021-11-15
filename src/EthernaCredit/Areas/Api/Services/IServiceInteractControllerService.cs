using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Api.Services
{
    public interface IServiceInteractControllerService
    {
        Task<double> GetUserBalanceAsync(string address);
        Task RegisterBalanceUpdateAsync(string clientId, string address, double ammount, string reason);
    }
}