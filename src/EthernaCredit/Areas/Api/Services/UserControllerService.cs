//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

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
