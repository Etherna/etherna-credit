using Etherna.Authentication.Extensions;
using Etherna.EthernaCredit.Areas.Withdraw.Pages;
using Etherna.EthernaCredit.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaCredit.Areas.Credit.Pages
{
    public class WithdrawModel : PageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            public string Ammount { get; set; } = default!;
        }

        // Fields.
        private readonly ICreditDbContext creditContext;

        // Constructor.
        public WithdrawModel(ICreditDbContext creditContext)
        {
            this.creditContext = creditContext;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public bool CanWithdraw => CreditBalance >= MinLimit && CreditBalance != 0;
        public double CreditBalance { get; private set; }
        public double MaxLimit => CreditBalance;
        public double MinLimit => WithdrawProcessModel.MinimumWithdraw;

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            var address = User.GetEtherAddress();
            var prevAddresses = User.GetEtherPrevAddresses();

            // Verify if user exists.
            var user = await creditContext.Users.QueryElementsAsync(elements =>
                elements.Where(u => u.Address == address ||
                                    prevAddresses.Contains(u.Address))
                        .FirstAsync());

            CreditBalance = user.CreditBalance;

            return Page();
        }

        public IActionResult OnPostAsync()
        {
            if (!ModelState.IsValid ||
                !double.TryParse(Input.Ammount, NumberStyles.Any, CultureInfo.InvariantCulture, out var ammountValue))
                return Page();

            return RedirectToPage("WithdrawProcess", new
            {
                area = "Withdraw",
                ammount = ammountValue
            });
        }
    }
}
