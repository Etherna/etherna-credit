using Etherna.EthernaCredit.Areas.Api.DtoModels;
using Etherna.EthernaCredit.Domain;
using Etherna.EthernaCredit.Extensions;
using Etherna.MongODM.Extensions;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaCredit.Areas.Api.Services
{
    public class UserControllerService : IUserControllerService
    {
        // Fields.
        private readonly ICreditContext creditContext;
        private readonly IHttpContextAccessor httpContextAccessor;

        // Constructor.
        public UserControllerService(
            ICreditContext creditContext,
            IHttpContextAccessor httpContextAccessor)
        {
            this.creditContext = creditContext;
            this.httpContextAccessor = httpContextAccessor;
        }

        // Methods.
        public async Task<double> GetCreditAsync()
        {
            var address = httpContextAccessor.HttpContext.User.GetEtherAddress();
            var user = await creditContext.Users.TryFindOneAsync(u => u.Address == address);
            return user?.CreditBalance ?? 0;
        }

        public async Task<IEnumerable<LogDto>> GetLogsAsync(int page, int take)
        {
            var address = httpContextAccessor.HttpContext.User.GetEtherAddress();
            return (await creditContext.OperationLogs.QueryElementsAsync(elements =>
                elements.Where(l => l.User.Address == address)
                        .PaginateDescending(u => u.CreationDateTime, page, take)
                        .ToListAsync()))
                        .Select(l => new LogDto(l));
        }
    }
}
