using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Admin.Pages.Users
{
    public class IndexModel : PageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            [Display(Name = "Ethereum address")]
            public string FindAddress { get; set; } = default!;
        }

        public class UserDto
        {
            public UserDto(User user, decimal balance)
            {
                if (user is null)
                    throw new ArgumentNullException(nameof(user));

                Id = user.Id;
                Balance = balance;
                EtherAddress = user.EtherAddress;
                HasUnlimitedCredit = user.HasUnlimitedCredit;
            }

            public string Id { get; }
            public decimal Balance { get; }
            public string EtherAddress { get; }
            public bool HasUnlimitedCredit { get; }
        }

        // Consts.
        private const int PageSize = 20;

        // Fields.
        private readonly ICreditDbContext dbContext;
        private readonly IUserService userService;

        // Constructor.
        public IndexModel(
            ICreditDbContext dbContext,
            IUserService userService)
        {
            this.dbContext = dbContext;
            this.userService = userService;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public int CurrentPage { get; private set; }
        public int MaxPage { get; private set; }
        public List<UserDto> Users { get; } = new();

        // Methods.
        public async Task OnGetAsync(int? p)
        {
            await InitializeAsync(p);
        }

        public async Task<IActionResult> OnPostAsync(int? p)
        {
            if (!ModelState.IsValid)
            {
                await InitializeAsync(p);
                return Page();
            }
            if (!Input.FindAddress.IsValidEthereumAddressHexFormat())
            {
                ModelState.AddModelError(string.Empty, "The value is not a valid Ethereum address");
                await InitializeAsync(p);
                return Page();
            }

            // Find user.
            var user = await userService.TryFindUserByAddressAsync(Input.FindAddress);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Can't find required user");
                await InitializeAsync(p);
                return Page();
            }

            return RedirectToPage("User", new { user.Id });
        }

        // Helpers.
        private async Task InitializeAsync(int? p)
        {
            CurrentPage = p ?? 0;

            var paginatedUsers = await dbContext.Users.QueryPaginatedElementsAsync(
                elements => elements, u => u.EtherAddress, CurrentPage, PageSize);

            foreach (var user in paginatedUsers.Elements)
            {
                var balance = await userService.GetUserBalanceAsync(user);
                Users.Add(new UserDto(user, balance));
            }

            MaxPage = paginatedUsers.MaxPage;
        }
    }
}
