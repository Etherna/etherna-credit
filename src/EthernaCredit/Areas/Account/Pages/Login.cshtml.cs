using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Etherna.CreditSystem.Areas.Account.Pages
{
    [Authorize]
    public class LoginModel : PageModel
    {
        // Methods.
        public IActionResult OnGetAsync(string? returnUrl = null) =>
             Redirect(returnUrl ?? Url.Content("~/"));
    }
}
