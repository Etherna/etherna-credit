using Etherna.Authentication.Extensions;
using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Events;
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
        private readonly IEventDispatcher eventDispatcher;
        private readonly IUserService userService;

        // Constructor.
        public DepositProcessModel(
            ICreditDbContext dbContext,
            IEventDispatcher eventDispatcher,
            IUserService userService)
        {
            this.dbContext = dbContext;
            this.eventDispatcher = eventDispatcher;
            this.userService = userService;
        }

        // Properties.
        public decimal DepositAmount { get; set; }
        public bool SucceededResult { get; set; }

        // Methods
        public IActionResult OnGet() =>
            LocalRedirect("~/");

        public async Task OnPostAsync(string amount)
        {
            if (amount is null)
                throw new ArgumentNullException(nameof(amount));

            // Get data.
            DepositAmount = decimal.Parse(amount.Trim('$'), CultureInfo.InvariantCulture);
            var (user, userSharedInfo) = await userService.FindUserAsync(User.GetEtherAddress());

            // Preliminary check.
            if (user.HasUnlimitedCredit) //disable deposit if unlimited credit
            {
                SucceededResult = false;
                return;
            }

            // Deposit.
            await userService.IncrementUserBalanceAsync(user, DepositAmount, false);

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
