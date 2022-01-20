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

using Etherna.Authentication.Extensions;
using Etherna.CreditSystem.Areas.Withdraw.Pages;
using Etherna.CreditSystem.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Manage.Pages
{
    public class WithdrawModel : PageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            public string Amount { get; set; } = default!;
        }

        // Fields.
        private readonly IUserService userService;

        // Constructor.
        public WithdrawModel(
            IUserService userService)
        {
            this.userService = userService;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;
        [TempData]
        public string? StatusMessage { get; set; }

        public bool CanWithdraw => CreditBalance >= MinLimit && CreditBalance != 0;
        public decimal CreditBalance { get; private set; }
        public decimal MaxLimit => CreditBalance;
        public decimal MinLimit => WithdrawProcessModel.MinimumWithdraw;

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            CreditBalance = await userService.GetUserBalanceAsync(User.GetEtherAddress());
            return Page();
        }

        public IActionResult OnPostAsync()
        {
            if (!ModelState.IsValid ||
                !double.TryParse(
                    Input.Amount.Trim('$'),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var amountValue))
            {
                StatusMessage = "Error, inserted value is not a valid number";
                return RedirectToPage();
            }

            return RedirectToPage("WithdrawProcess", new
            {
                area = "Withdraw",
                amount = amountValue
            });
        }
    }
}
