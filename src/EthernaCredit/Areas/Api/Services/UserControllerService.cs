using Etherna.Authentication.Extensions;
using Etherna.CreditSystem.Areas.Api.DtoModels;
using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Services.Domain;
using Etherna.MongoDB.Driver.Linq;
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
        public string GetAddress(ClaimsPrincipal user) =>
            user.GetEtherAddress();

        public async Task<CreditDto> GetCreditAsync(ClaimsPrincipal user)
        {
            var (userModel, _) = await userService.FindUserAsync(user.GetEtherAddress());
            var balance = await userService.GetUserBalanceAsync(userModel);

            return new CreditDto(balance, userModel.HasUnlimitedCredit);
        }

        public async Task<IEnumerable<OperationLogDto>> GetLogsAsync(ClaimsPrincipal user, int page, int take)
        {
            var (userModel, userSharedInfo) = await userService.FindUserAsync(user.GetEtherAddress());
            var paginatedLogs = await dbContext.OperationLogs.QueryPaginatedElementsAsync(
                elements => elements.Where(l => l.User.Id == userModel.Id),
                l => l.CreationDateTime,
                page,
                take);

            return paginatedLogs.Elements.Select(l => new OperationLogDto(l, userSharedInfo));
        }
    }
}
