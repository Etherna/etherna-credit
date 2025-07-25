// Copyright 2021-present Etherna SA
// This file is part of Etherna Credit.
// 
// Etherna Credit is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Credit is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Credit.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.Authentication;
using Etherna.Credit.Areas.Api.DtoModels;
using Etherna.Credit.Domain;
using Etherna.Credit.Services.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.Credit.Areas.Api.Services
{
    internal sealed class UserControllerService(
        ICreditDbContext dbContext,
        IEthernaOpenIdConnectClient ethernaOidcClient,
        IUserService userService)
        : IUserControllerService
    {
        // Methods.
        public Task<string> GetAddressAsync() =>
            ethernaOidcClient.GetEtherAddressAsync();

        public async Task<CreditDto> GetCreditAsync()
        {
            var (userModel, _) = await userService.FindUserAsync(await ethernaOidcClient.GetEtherAddressAsync());
            var balance = await userService.GetUserBalanceAsync(userModel);

            return new CreditDto(balance, userModel.HasUnlimitedCredit);
        }

        public async Task<IEnumerable<OperationLogDto>> GetLogsAsync(int page, int take)
        {
            var (userModel, userSharedInfo) = await userService.FindUserAsync(await ethernaOidcClient.GetEtherAddressAsync());
            var paginatedLogs = await dbContext.OperationLogs.QueryPaginatedElementsAsync(
                elements => elements.Where(l => l.User.Id == userModel.Id),
                l => l.CreationDateTime,
                page,
                take);

            return paginatedLogs.Elements.Select(l => new OperationLogDto(l, userSharedInfo));
        }
    }
}
