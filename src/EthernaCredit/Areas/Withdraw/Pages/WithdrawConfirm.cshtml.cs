using Etherna.EthernaCredit.Domain;
using Etherna.EthernaCredit.Domain.Models;
using Etherna.EthernaCredit.Domain.Models.OperationLogs;
using Etherna.EthernaCredit.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Etherna.EthernaCredit.Areas.Withdraw.Pages
{
    public class WithdrawConfirmModel : PageModel
    {
        // Fields.
        private readonly ICreditContext creditContext;

        // Constructor.
        public WithdrawConfirmModel(ICreditContext creditContext)
        {
            this.creditContext = creditContext;
        }

        // Properties.
        public bool SucceededResult { get; set; }
        public double WithdrawAmmount { get; set; }

        // Methods
        public IActionResult OnGet() =>
            LocalRedirect("~/");

        public async Task OnPostAsync(string ammount)
        {
            if (ammount is null)
                throw new ArgumentNullException(nameof(ammount));

            // Get data.
            var ammountValue = double.Parse(ammount.Trim('$'), CultureInfo.InvariantCulture);
            var address = User.GetEtherAddress();
            var user = await creditContext.Users.FindOneAsync(u => u.Address == address);

            // Deposit.
            await creditContext.Users.Collection.FindOneAndUpdateAsync(
                Builders<User>.Filter.Eq(u => u.Address, address),
                Builders<User>.Update.Inc(u => u.CreditBalance, -ammountValue));

            // Report log.
            var depositLog = new WithdrawOperationLog(ammountValue, address, user);
            await creditContext.OperationLogs.CreateAsync(depositLog);

            WithdrawAmmount = ammountValue;
        }
    }
}
