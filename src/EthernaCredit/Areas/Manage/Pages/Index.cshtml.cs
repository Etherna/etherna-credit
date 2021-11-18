using Etherna.CreditSystem.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Manage.Pages
{
    public class IndexModel : PageModel
    {
        // Fields.
        private readonly IUserService userService;

        // Constructor.
        public IndexModel(
            IUserService userService)
        {
            this.userService = userService;
        }

        // Properties.
        [Display(Name = "Credit balance")]
        public double CreditBalance { get; set; }
        [Display(Name = "Ethereum address")]
        public string EthereumAddress { get; set; } = default!;
        [TempData]
        public string? StatusMessage { get; set; }

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            // Get user.
            var user = await userService.FindAndUpdateUserAsync(User);

            EthereumAddress = user.EtherAddress;
            CreditBalance = await userService.GetUserBalanceAsync(user);

            return Page();
        }
    }
}
