using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.OperationLogs;
using Etherna.CreditSystem.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Deposit.Pages
{
    public class DepositProcessModel : PageModel
    {
        // Fields.
        private readonly ICreditDbContext creditContext;
        private readonly IUserService userService;

        // Constructor.
        public DepositProcessModel(
            ICreditDbContext creditContext,
            IUserService userService)
        {
            this.creditContext = creditContext;
            this.userService = userService;
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
            var user = await userService.FindAndUpdateUserAsync(User);

            // Deposit.
            await creditContext.Users.Collection.FindOneAndUpdateAsync(
                u => u.Address == user.Address,
                Builders<User>.Update.Inc(u => u.CreditBalance, ammountValue));

            // Report log.
            var depositLog = new DepositOperationLog(ammountValue, user.Address, user);
            await creditContext.OperationLogs.CreateAsync(depositLog);

            DepositAmmount = ammountValue;
        }
    }
}
