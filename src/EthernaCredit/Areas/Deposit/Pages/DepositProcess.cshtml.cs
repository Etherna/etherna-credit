using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models.OperationLogs;
using Etherna.CreditSystem.Services.Domain;
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
        private readonly IUserService userService;

        // Constructor.
        public DepositProcessModel(
            ICreditDbContext dbContext,
            IUserService userService)
        {
            this.dbContext = dbContext;
            this.userService = userService;
        }

        // Properties.
        public double DepositAmmount { get; set; }
        public bool SucceededResult { get; set; }

        // Methods
        public IActionResult OnGet() =>
            LocalRedirect("~/");

        public async Task OnPostAsync(string ammount)
        {
            if (ammount is null)
                throw new ArgumentNullException(nameof(ammount));

            // Get data.
            DepositAmmount = double.Parse(ammount.Trim('$'), CultureInfo.InvariantCulture);
            var user = await userService.FindAndUpdateUserAsync(User);

            // Preliminary check.
            if (user.HasUnlimitedCredit) //disable deposit if unlimited credit
            {
                SucceededResult = false;
                return;
            }

            // Deposit.
            await userService.IncrementUserBalanceAsync(user, DepositAmmount, false);

            // Report log.
            var depositLog = new DepositOperationLog(DepositAmmount, user.EtherAddress, user);
            await dbContext.OperationLogs.CreateAsync(depositLog);

            SucceededResult = true;
        }
    }
}
