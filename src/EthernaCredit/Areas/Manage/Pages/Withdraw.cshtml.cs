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

using Etherna.Authentication;
using Etherna.CreditSystem.Areas.Withdraw.Pages;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Manage.Pages
{
    public class WithdrawModel : PageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            public XDaiBalance Amount { get; set; }
        }

        // Fields.
        private readonly IEthernaOpenIdConnectClient ethernaOidcClient;
        private readonly IUserService userService;

        // Constructor.
        public WithdrawModel(
            IEthernaOpenIdConnectClient ethernaOidcClient,
            IUserService userService)
        {
            this.ethernaOidcClient = ethernaOidcClient;
            this.userService = userService;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;
        [TempData]
        public string? StatusMessage { get; set; }

        public bool CanWithdraw => CreditBalance >= MinLimit && CreditBalance != 0;
        public XDaiBalance CreditBalance { get; private set; }
        public XDaiBalance MaxLimit => CreditBalance;
        public XDaiBalance MinLimit => WithdrawProcessModel.MinimumWithdraw;

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            CreditBalance = await userService.GetUserBalanceAsync(await ethernaOidcClient.GetEtherAddressAsync());
            return Page();
        }

        public IActionResult OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                StatusMessage = "Error, inserted value is not a valid number";
                return RedirectToPage();
            }

            return RedirectToPage("WithdrawProcess", new
            {
                area = "Withdraw",
                amount = Input.Amount
            });
        }
    }
}
