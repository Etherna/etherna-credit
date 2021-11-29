using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Admin.Pages.Users
{
    public class UserModel : PageModel
    {
        // Models.
        public class InputModel
        {
            public bool HasUnlimitedCredit { get; set; }
        }

        // Fields.
        private readonly ICreditDbContext dbContext;
        private readonly IUserService userService;

        // Constructor.
        public UserModel(
            ICreditDbContext dbContext,
            IUserService userService)
        {
            this.dbContext = dbContext;
            this.userService = userService;
        }

        // Properties.
        public string Id { get; private set; } = default!;

        public decimal Balance { get; private set; }

        [Display(Name = "Ethereum address")]
        public string? EtherAddress { get; private set; }

        [Display(Name = "Previous ethereum addresses")]
        public IEnumerable<string> EtherPreviousAddresses { get; private set; } = Array.Empty<string>();

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        // Methods.
        public async Task OnGetAsync(string id)
        {
            var user = await dbContext.Users.FindOneAsync(id);

            Id = id;
            Balance = await userService.GetUserBalanceAsync(user);
            EtherAddress = user.EtherAddress;
            EtherPreviousAddresses = user.EtherPreviousAddresses;
            Input = new InputModel { HasUnlimitedCredit = user.HasUnlimitedCredit };
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var user = await dbContext.Users.FindOneAsync(id);

            user.HasUnlimitedCredit = Input.HasUnlimitedCredit;
            await dbContext.SaveChangesAsync();

            return RedirectToPage(new { id });
        }
    }
}
