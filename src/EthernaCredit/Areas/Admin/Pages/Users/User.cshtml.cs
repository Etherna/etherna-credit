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
using Etherna.Credit.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.Credit.Areas.Admin.Pages.Users
{
    public class UserModel(
        ICreditDbContext creditDbContext,
        ISharedDbContext sharedDbContext,
        IUserService userService)
        : PageModel
    {
        // Models.
        public class InputModel
        {
            public bool HasUnlimitedCredit { get; set; }
        }

        // Properties.
        public string Id { get; private set; } = null!;

        public XDaiValue Balance { get; private set; }

        [Display(Name = "Ethereum address")]
        public EthAddress? EtherAddress { get; private set; }

        [Display(Name = "Previous ethereum addresses")]
        public IEnumerable<EthAddress> EtherPreviousAddresses { get; private set; } = [];

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        // Methods.
        public async Task OnGetAsync(string id)
        {
            var user = await creditDbContext.Users.FindOneAsync(id);
            var userSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(user.SharedInfoId);

            Id = id;
            Balance = await userService.GetUserBalanceAsync(user);
            EtherAddress = userSharedInfo.EtherAddress;
            EtherPreviousAddresses = userSharedInfo.EtherPreviousAddresses;
            Input = new InputModel { HasUnlimitedCredit = user.HasUnlimitedCredit };
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var user = await creditDbContext.Users.FindOneAsync(id);

            user.HasUnlimitedCredit = Input.HasUnlimitedCredit;
            await creditDbContext.SaveChangesAsync();

            return RedirectToPage(new { id });
        }
    }
}
