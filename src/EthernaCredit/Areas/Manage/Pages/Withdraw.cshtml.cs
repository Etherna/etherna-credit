using Etherna.Authentication.Extensions;
using Etherna.CreditSystem.Areas.Withdraw.Pages;
using Etherna.CreditSystem.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Manage.Pages
{
    public class WithdrawModel : PageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            public string Amount { get; set; } = default!;
        }

        // Fields.
        private readonly IUserService userService;

        // Constructor.
        public WithdrawModel(
            IUserService userService)
        {
            this.userService = userService;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;
        [TempData]
        public string? StatusMessage { get; set; }

        public bool CanWithdraw => CreditBalance >= MinLimit && CreditBalance != 0;
        public decimal CreditBalance { get; private set; }
        public decimal MaxLimit => CreditBalance;
        public decimal MinLimit => WithdrawProcessModel.MinimumWithdraw;

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            CreditBalance = await userService.GetUserBalanceAsync(User.GetEtherAddress());
            return Page();
        }

        public IActionResult OnPostAsync()
        {
            if (!ModelState.IsValid ||
                !double.TryParse(
                    Input.Amount.Trim('$'),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var amountValue))
            {
                StatusMessage = "Error, inserted value is not a valid number";
                return RedirectToPage();
            }

            return RedirectToPage("WithdrawProcess", new
            {
                area = "Withdraw",
                amount = amountValue
            });
        }
    }
}
