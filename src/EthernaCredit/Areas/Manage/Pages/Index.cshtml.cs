using Etherna.Authentication.Extensions;
using Etherna.CreditSystem.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver.Linq;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Credit.Pages
{
    public class IndexModel : PageModel
    {
        // Fields.
        private readonly ICreditDbContext creditContext;

        // Constructor.
        public IndexModel(ICreditDbContext creditContext)
        {
            this.creditContext = creditContext;
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
            var address = User.GetEtherAddress();
            var prevAddresses = User.GetEtherPrevAddresses();

            // Verify if user exists.
            var user = await creditContext.Users.QueryElementsAsync(elements =>
                elements.Where(u => u.Address == address ||
                                    prevAddresses.Contains(u.Address))
                        .FirstAsync());

            EthereumAddress = user.Address;
            CreditBalance = user.CreditBalance;

            return Page();
        }
    }
}
