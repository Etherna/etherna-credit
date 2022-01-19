using Etherna.CreditSystem.Areas.Api.DtoModels;
using System;
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
            DateTime? fromDate,
            DateTime? toDate);

        Task RegisterBalanceUpdateAsync(
            string clientId,
            string address,
            decimal amount,
            string reason);
    }
}