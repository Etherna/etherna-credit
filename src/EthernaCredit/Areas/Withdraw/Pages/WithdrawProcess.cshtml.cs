//   Copyright 2021-present Etherna Sa
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

using Etherna.Authentication;
using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Events;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.OperationLogs;
using Etherna.CreditSystem.Services.Domain;
using Etherna.DomainEvents;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Withdraw.Pages
{
    public class WithdrawProcessModel : PageModel
    {
        // Consts.
        public static readonly XDaiBalance MinimumWithdraw = 1.0M;

        // Fields.
        private readonly ICreditDbContext dbContext;
        private readonly IEthernaOpenIdConnectClient ethernaOidcClient;
        private readonly IEventDispatcher eventDispatcher;
        private readonly IUserService userService;

        // Constructor.
        public WithdrawProcessModel(
            ICreditDbContext dbContext,
            IEthernaOpenIdConnectClient ethernaOidcClient,
            IEventDispatcher eventDispatcher,
            IUserService userService)
        {
            this.dbContext = dbContext;
            this.ethernaOidcClient = ethernaOidcClient;
            this.eventDispatcher = eventDispatcher;
            this.userService = userService;
        }

        // Properties.
        public bool SucceededResult { get; set; }
        public XDaiBalance WithdrawAmount { get; set; }

        // Methods
        public async Task OnGetAsync(XDaiBalance amount)
        {
            ArgumentNullException.ThrowIfNull(amount, nameof(amount));

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
            var withdrawLog = new WithdrawOperationLog(-WithdrawAmount, userSharedInfo.EtherAddress, user);
            await dbContext.OperationLogs.CreateAsync(withdrawLog);

            // Dispatch event.
            await eventDispatcher.DispatchAsync(new UserWithdrawEvent(withdrawLog));
        }
    }
}
