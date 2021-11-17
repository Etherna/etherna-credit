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
            public string Ammount { get; set; } = default!;
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

        public bool CanWithdraw => CreditBalance >= MinLimit && CreditBalance != 0;
        public double CreditBalance { get; private set; }
        public double MaxLimit => CreditBalance;
        public double MinLimit => WithdrawProcessModel.MinimumWithdraw;

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            // Get user.
            var user = await userService.FindAndUpdateUserAsync(User);

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
