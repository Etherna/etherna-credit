using Etherna.Authentication.Extensions;
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
        public decimal CreditBalance { get; private set; }
        [Display(Name = "Ethereum address")]
        public string EthereumAddress { get; private set; } = default!;
        public bool HasUnlimitedCredit { get; private set; }

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            // Get user.
            var (user, userSharedInfo) = await userService.FindUserAsync(User.GetEtherAddress());

            EthereumAddress = userSharedInfo.EtherAddress;
            CreditBalance = await userService.GetUserBalanceAsync(user);
            HasUnlimitedCredit = user.HasUnlimitedCredit;

            return Page();
        }
    }
}
