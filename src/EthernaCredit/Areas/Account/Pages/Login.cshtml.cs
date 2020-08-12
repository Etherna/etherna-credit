using Etherna.EthernaCredit.Domain;
using Etherna.EthernaCredit.Domain.Models;
using Etherna.EthernaCredit.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaCredit.Areas.Account.Pages
{
    [Authorize]
    public class LoginModel : PageModel
    {
        // Fields.
        private readonly ICreditContext creditContext;

        // Constructors.
        public LoginModel(ICreditContext creditContext)
        {
            this.creditContext = creditContext;
        }

        // Methods.
        public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var address = User.GetEtherAddress();
            var prevAddresses = User.GetEtherPrevAddresses();

            // Verify if user exists.
            var user = await creditContext.Users.QueryElementsAsync(elements =>
                elements.Where(u => u.Address == address ||
                                    prevAddresses.Contains(u.Address))
                        .FirstOrDefaultAsync());

            // Create if it doesn't exist.
            if (user is null)
            {
                user = new User(address);
                await creditContext.Users.CreateAsync(user);
            }

            // Check if user have changed address.
            if (address != user.Address)
            {
                //migrate
                throw new NotImplementedException();
            }

            return Redirect(returnUrl);
        }
    }
}
