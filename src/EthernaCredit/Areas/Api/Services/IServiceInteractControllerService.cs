using System.Threading.Tasks;

namespace Etherna.EthernaCredit.Areas.Api.Services
{
    public interface IServiceInteractControllerService
    {
        Task<double> GetUserBalanceAsync(string address);
        Task RegisterBalanceUpdateAsync(string address, double ammount, string reason);
    }
}