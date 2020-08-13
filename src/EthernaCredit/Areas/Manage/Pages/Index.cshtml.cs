using Etherna.EthernaCredit.Domain;
using Etherna.EthernaCredit.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver.Linq;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaCredit.Areas.Credit.Pages
{
    public class IndexModel : PageModel
    {
        // Fields.
        private readonly ICreditContext creditContext;

        // Constructor.
        public IndexModel(ICreditContext creditContext)
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
            await LoadAsync();
            return Page();
        }

        // Helpers.
        private async Task LoadAsync()
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
        }
    }
}
