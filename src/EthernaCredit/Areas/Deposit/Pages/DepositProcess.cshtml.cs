using Etherna.Authentication.Extensions;
using Etherna.EthernaCredit.Domain;
using Etherna.EthernaCredit.Domain.Models;
using Etherna.EthernaCredit.Domain.Models.OperationLogs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Etherna.EthernaCredit.Areas.Deposit.Pages
{
    public class DepositProcessModel : PageModel
    {
        // Fields.
        private readonly ICreditContext creditContext;

        // Constructor.
        public DepositProcessModel(ICreditContext creditContext)
        {
            this.creditContext = creditContext;
        }

        // Properties.
        public double DepositAmmount { get; set; }

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
                u => u.Address == address,
                Builders<User>.Update.Inc(u => u.CreditBalance, ammountValue));

            // Report log.
            var depositLog = new DepositOperationLog(ammountValue, address, user);
            await creditContext.OperationLogs.CreateAsync(depositLog);

            DepositAmmount = ammountValue;
        }
    }
}
