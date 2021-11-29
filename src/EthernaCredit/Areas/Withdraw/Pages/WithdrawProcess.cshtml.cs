using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Events;
using Etherna.CreditSystem.Domain.Models.OperationLogs;
using Etherna.CreditSystem.Services.Domain;
using Etherna.DomainEvents;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Withdraw.Pages
{
    public class WithdrawProcessModel : PageModel
    {
        // Consts.
        public const decimal MinimumWithdraw = 1.0M;

        // Fields.
        private readonly ICreditDbContext dbContext;
        private readonly IEventDispatcher eventDispatcher;
        private readonly IUserService userService;

        // Constructor.
        public WithdrawProcessModel(
            ICreditDbContext dbContext,
            IEventDispatcher eventDispatcher,
            IUserService userService)
        {
            this.dbContext = dbContext;
            this.eventDispatcher = eventDispatcher;
            this.userService = userService;
        }

        // Properties.
        public bool SucceededResult { get; set; }
        public decimal WithdrawAmmount { get; set; }

        // Methods
        public async Task OnGetAsync(string ammount)
        {
            if (ammount is null)
                throw new ArgumentNullException(nameof(ammount));

            // Get data.
            WithdrawAmmount = decimal.Parse(ammount.Trim('$'), CultureInfo.InvariantCulture);
            var user = await userService.FindAndUpdateUserAsync(User); //create or update address, if required

            // Preliminary check.
            if (user.HasUnlimitedCredit || //***** disable withdraw if unlimited credit (SECURITY!) *****
                WithdrawAmmount < MinimumWithdraw)
            {
                SucceededResult = false;
                return;
            }

            // Update user balance.
            SucceededResult = await userService.IncrementUserBalanceAsync(user, -WithdrawAmmount, false);
            if (!SucceededResult)
                return;

            // Report log.
            var withdrawLog = new WithdrawOperationLog(-WithdrawAmmount, user.EtherAddress, user);
            await dbContext.OperationLogs.CreateAsync(withdrawLog);

            // Dispatch event.
            await eventDispatcher.DispatchAsync(new UserWithdrawEvent(
                -WithdrawAmmount, user));
        }
    }
}
