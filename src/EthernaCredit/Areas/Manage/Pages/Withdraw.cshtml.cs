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
using Etherna.BeeNet.Models;
using Etherna.Credit.Areas.Withdraw.Pages;
using Etherna.Credit.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.Credit.Areas.Manage.Pages
{
    public class WithdrawModel(
        IEthernaOpenIdConnectClient ethernaOidcClient,
        IUserService userService)
        : PageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            public XDaiValue Amount { get; set; }
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;
        [TempData]
        public string? StatusMessage { get; set; }

        public bool CanWithdraw => CreditBalance >= MinLimit && CreditBalance != 0;
        public XDaiValue CreditBalance { get; private set; }
        public XDaiValue MaxLimit => CreditBalance;
        public XDaiValue MinLimit => WithdrawProcessModel.MinimumWithdraw;

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
