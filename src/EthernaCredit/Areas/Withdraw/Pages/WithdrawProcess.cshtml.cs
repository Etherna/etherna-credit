using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.OperationLogs;
using Etherna.CreditSystem.Services.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Withdraw.Pages
{
    public class WithdrawProcessModel : PageModel
    {
        // Consts.
        public const double MinimumWithdraw = 1.0;

        // Fields.
        private readonly ICreditDbContext dbContext;
        private readonly IUserService userService;

        // Constructor.
        public WithdrawProcessModel(
            ICreditDbContext dbContext,
            IUserService userService)
        {
            this.dbContext = dbContext;
            this.userService = userService;
        }

        // Properties.
        public bool SucceededResult { get; set; }
        public double WithdrawAmmount { get; set; }

        // Methods
        public async Task OnGetAsync(string ammount)
        {
            if (ammount is null)
                throw new ArgumentNullException(nameof(ammount));

            // Get data.
            WithdrawAmmount = double.Parse(ammount.Trim('$'), CultureInfo.InvariantCulture);
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
        }
    }
}
