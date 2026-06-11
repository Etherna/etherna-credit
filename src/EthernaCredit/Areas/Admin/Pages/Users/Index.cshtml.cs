// Copyright 2021-present Etherna SA
// This file is part of Etherna Credit.
// 
// Etherna Credit is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Credit is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Credit.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.BeeNet.Models;
using Etherna.Credit.Domain;
using Etherna.Credit.Domain.Models;
using Etherna.Credit.Domain.Models.UserAgg;
using Etherna.Credit.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.Credit.Areas.Admin.Pages.Users
{
    public class IndexModel(
        ISharedDbContext sharedDbContext,
        IUserService userService)
        : PageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            [Display(Name = "Ethereum address")]
            public string FindAddress { get; set; } = null!;
        }

        public class UserDto
        {
            public UserDto(User user, UserSharedInfo userSharedInfo, XDaiValue balance)
            {
                ArgumentNullException.ThrowIfNull(user);
                ArgumentNullException.ThrowIfNull(userSharedInfo);

                Id = user.Id;
                Balance = balance;
                EtherAddress = userSharedInfo.EtherAddress;
                HasUnlimitedCredit = user.HasUnlimitedCredit;
            }

            public string Id { get; }
            public XDaiValue Balance { get; }
            public EthAddress EtherAddress { get; }
            public bool HasUnlimitedCredit { get; }
        }

        // Consts.
        private const int PageSize = 20;

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = null!;

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
