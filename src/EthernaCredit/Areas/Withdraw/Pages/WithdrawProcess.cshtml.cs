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
        private readonly ICreditDbContext creditContext;
        private readonly IUserService userService;

        // Constructor.
        public WithdrawProcessModel(
            ICreditDbContext creditContext,
            IUserService userService)
        {
            this.creditContext = creditContext;
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
            if (WithdrawAmmount < MinimumWithdraw)
            {
                SucceededResult = false;
                return;
            }

            // Update user balance.
            var userResult = await creditContext.Users.Collection.FindOneAndUpdateAsync(
                u => u.Id == user.Id &&
                     u.CreditBalance >= WithdrawAmmount, //verify disponibility
                Builders<User>.Update.Inc(u => u.CreditBalance, -WithdrawAmmount));

            // Result check.
            if (userResult is null)
            {
                SucceededResult = false;
                return;
            }
            SucceededResult = true;

            // Report log.
            var withdrawLog = new WithdrawOperationLog(-WithdrawAmmount, user.Address, user);
            await creditContext.OperationLogs.CreateAsync(withdrawLog);
        }
    }
}
