using Etherna.Authentication.Extensions;
using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.OperationLogs;
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

        // Constructor.
        public WithdrawProcessModel(ICreditDbContext creditContext)
        {
            this.creditContext = creditContext;
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
            var address = User.GetEtherAddress();

            // Preliminary check.
            if (WithdrawAmmount < MinimumWithdraw)
            {
                SucceededResult = false;
                return;
            }

            // Withdraw.
            var user = await creditContext.Users.Collection.FindOneAndUpdateAsync(
                u => u.Address == address &&
                     u.CreditBalance >= WithdrawAmmount, //verify disponibility
                Builders<User>.Update.Inc(u => u.CreditBalance, -WithdrawAmmount));

            // Result check.
            if (user is null)
            {
                SucceededResult = false;
                return;
            }
            SucceededResult = true;

            // Report log.
            var withdrawLog = new WithdrawOperationLog(-WithdrawAmmount, address, user);
            await creditContext.OperationLogs.CreateAsync(withdrawLog);
        }
    }
}
