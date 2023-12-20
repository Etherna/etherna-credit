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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Deposit.Pages
{
    public class DepositProcessModel : PageModel
    {
        // Fields.
        private readonly ICreditDbContext dbContext;
        private readonly IEthernaOpenIdConnectClient ethernaOidcClient;
        private readonly IEventDispatcher eventDispatcher;
        private readonly IUserService userService;

        // Constructor.
        public DepositProcessModel(
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
        public XDaiBalance DepositAmount { get; set; }
        public bool SucceededResult { get; set; }

        // Methods
        public IActionResult OnGet() =>
            LocalRedirect("~/");

        public async Task OnPostAsync(string amount)
        {
            ArgumentNullException.ThrowIfNull(amount, nameof(amount));

            // Get data.
            DepositAmount = decimal.Parse(amount, CultureInfo.InvariantCulture);
            var (user, userSharedInfo) = await userService.FindUserAsync(await ethernaOidcClient.GetEtherAddressAsync());

            // Preliminary check.
            if (DepositAmount <= 0)
            {
                SucceededResult = false;
                return;
            }
            if (user.HasUnlimitedCredit) //disable deposit if unlimited credit
            {
                SucceededResult = false;
                return;
            }

            // Deposit.
            await userService.TryIncrementUserBalanceAsync(user, DepositAmount, false);

            // Report log.
            var depositLog = new DepositOperationLog(DepositAmount, userSharedInfo.EtherAddress, user);
            await dbContext.OperationLogs.CreateAsync(depositLog);

            // Dispatch event.
            await eventDispatcher.DispatchAsync(new UserDepositEvent(
                DepositAmount, user));

            SucceededResult = true;
        }
    }
}
