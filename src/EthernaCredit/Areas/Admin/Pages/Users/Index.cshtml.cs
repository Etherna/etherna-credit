//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.UserAgg;
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
            public UserDto(User user, UserSharedInfo userSharedInfo, decimal balance)
            {
                if (user is null)
                    throw new ArgumentNullException(nameof(user));
                if (userSharedInfo is null)
                    throw new ArgumentNullException(nameof(userSharedInfo));

                Id = user.Id;
                Balance = balance;
                EtherAddress = userSharedInfo.EtherAddress;
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
        private readonly ISharedDbContext sharedDbContext;
        private readonly IUserService userService;

        // Constructor.
        public IndexModel(
            ISharedDbContext sharedDbContext,
            IUserService userService)
        {
            this.sharedDbContext = sharedDbContext;
            this.userService = userService;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public int CurrentPage { get; private set; }
        public long MaxPage { get; private set; }
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
            var (user, _) = await userService.TryFindUserAsync(Input.FindAddress);

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

            var paginatedUsersSharedInfo = await sharedDbContext.UsersInfo.QueryPaginatedElementsAsync(
                elements => elements, u => u.EtherAddress, CurrentPage, PageSize);

            foreach (var sharedInfo in paginatedUsersSharedInfo.Elements)
            {
                var (user, _) = await userService.FindUserAsync(sharedInfo);
                var balance = await userService.GetUserBalanceAsync(user);
                Users.Add(new UserDto(user, sharedInfo, balance));
            }

            MaxPage = paginatedUsersSharedInfo.MaxPage;
        }
    }
}
