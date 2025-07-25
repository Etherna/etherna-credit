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
using Etherna.Credit.Domain.Models;
using Etherna.Credit.Domain.Models.OperationLogs;
using Etherna.Credit.Services.Domain;
using Etherna.DomainEvents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Etherna.Credit.Areas.Deposit.Pages
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
            await eventDispatcher.DispatchAsync(new UserDepositEvent(depositLog));

            SucceededResult = true;
        }
    }
}
