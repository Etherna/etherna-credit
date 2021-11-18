using Etherna.CreditSystem.Areas.Api.DtoModels;
using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Services.Domain;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Api.Services
{
    public class UserControllerService : IUserControllerService
    {
        // Fields.
        private readonly ICreditDbContext dbContext;
        private readonly IUserService userService;

        // Constructor.
        public UserControllerService(
            ICreditDbContext dbContext,
            IUserService userService)
        {
            this.dbContext = dbContext;
            this.userService = userService;
        }

        // Methods.
        public Task<double> GetCreditAsync(ClaimsPrincipal user) =>
            userService.GetUserBalanceAsync(user);

        public async Task<IEnumerable<LogDto>> GetLogsAsync(ClaimsPrincipal user, int page, int take)
        {
            var userModel = await userService.FindAndUpdateUserAsync(user);
            var paginatedLogs = await dbContext.OperationLogs.QueryPaginatedElementsAsync(
                elements => elements.Where(l => l.User.Id == userModel.Id),
                l => l.CreationDateTime,
                page,
                take);

            return paginatedLogs.Elements.Select(l => new LogDto(l));
        }
    }
}
