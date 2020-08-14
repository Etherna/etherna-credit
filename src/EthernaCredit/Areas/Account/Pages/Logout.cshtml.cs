using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Etherna.EthernaCredit.Areas.Account.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet() =>
            SignOut("Cookies", "oidc");
    }
}