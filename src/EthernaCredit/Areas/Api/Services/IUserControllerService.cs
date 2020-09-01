using Etherna.EthernaCredit.Areas.Api.DtoModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaCredit.Areas.Api.Services
{
    public interface IUserControllerService
    {
        Task<IEnumerable<LogDto>> GetLogsAsync(int page, int take);
        Task<double> GetCreditAsync();
    }
}