using Etherna.Authentication.Extensions;
using Etherna.EthernaCredit.Areas.Api.DtoModels;
using Etherna.EthernaCredit.Domain;
using Etherna.MongODM.Core.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.EthernaCredit.Areas.Api.Services
{
    public class UserControllerService : IUserControllerService
    {
        // Fields.
        private readonly ICreditDbContext creditContext;

        // Constructor.
        public UserControllerService(
            ICreditDbContext creditContext)
        {
            this.creditContext = creditContext;
        }

        // Methods.
        public async Task<double> GetCreditAsync(ClaimsPrincipal user)
        {
            var address = user.GetEtherAddress();
            var userModel = await creditContext.Users.TryFindOneAsync(u => u.Address == address);
            return userModel?.CreditBalance ?? 0;
        }

        public async Task<IEnumerable<LogDto>> GetLogsAsync(ClaimsPrincipal user, int page, int take)
        {
            var address = user.GetEtherAddress();
            return (await creditContext.OperationLogs.QueryElementsAsync(elements =>
                elements.Where(l => l.User.Address == address)
                        .PaginateDescending(u => u.CreationDateTime, page, take)
                        .ToListAsync()))
                        .Select(l => new LogDto(l));
        }
    }
}
