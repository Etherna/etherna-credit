//   Copyright 2021-present Etherna Sa
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
        private readonly ICreditDbContext creditDbContext;
        private readonly ISharedDbContext sharedDbContext;
        private readonly IUserService userService;

        // Constructor.
        public UserModel(
            ICreditDbContext creditDbContext,
            ISharedDbContext sharedDbContext,
            IUserService userService)
        {
            this.creditDbContext = creditDbContext;
            this.sharedDbContext = sharedDbContext;
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
