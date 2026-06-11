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
using Etherna.Credit.Domain;
using Etherna.Credit.Domain.Events;
using Etherna.Credit.Domain.Models.OperationLogs;
using Etherna.Credit.Services.Domain;
using Etherna.DomainEvents;
using Etherna.SwarmSdk.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Etherna.Credit.Areas.Withdraw.Pages
{
    public class WithdrawProcessModel(
        ICreditDbContext dbContext,
        IEthernaOpenIdConnectClient ethernaOidcClient,
        IEventDispatcher eventDispatcher,
        IUserService userService)
        : PageModel
    {
        // Consts.
        public static readonly XDaiValue MinimumWithdraw = 1.0M;

        // Properties.
        public bool SucceededResult { get; set; }
        public XDaiValue WithdrawAmount { get; set; }

        // Methods
        public async Task OnGetAsync(XDaiValue amount)
        {
            // Get data.
            WithdrawAmount = amount;
            WithdrawAmount = decimal.Truncate(WithdrawAmount.ToDecimal() * 100) / 100; //accept 2 digit precision

            var (user, userSharedInfo) = await userService.FindUserAsync(await ethernaOidcClient.GetEtherAddressAsync());

            // Preliminary check.
            if (user.HasUnlimitedCredit || //***** disable withdraw if unlimited credit (SECURITY!) *****
                WithdrawAmount < MinimumWithdraw)
            {
                SucceededResult = false;
                return;
            }

            // Update user balance.
            SucceededResult = await userService.TryIncrementUserBalanceAsync(user, -WithdrawAmount, false);
            if (!SucceededResult)
                return;

            // Report log.
            var withdrawLog = new WithdrawOperationLog(
                -WithdrawAmount,
                userSharedInfo.EtherAddress.ToString(),
                user);
            await dbContext.OperationLogs.CreateAsync(withdrawLog);

            // Dispatch event.
            await eventDispatcher.DispatchAsync(new UserWithdrawEvent(withdrawLog));
        }
    }
}
