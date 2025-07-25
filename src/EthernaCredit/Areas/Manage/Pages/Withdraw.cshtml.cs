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

using Etherna.Authentication;
using Etherna.Credit.Areas.Withdraw.Pages;
using Etherna.Credit.Domain.Models;
using Etherna.Credit.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.Credit.Areas.Manage.Pages
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
