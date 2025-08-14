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
using Etherna.Credit.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.Credit.Areas.Manage.Pages
{
    public class IndexModel(
        IEthernaOpenIdConnectClient ethernaOidcClient,
        IUserService userService)
        : PageModel
    {
        // Properties.
        [Display(Name = "Credit balance")]
        public XDaiValue CreditBalance { get; private set; }
        [Display(Name = "Ethereum address")]
        public string EthereumAddress { get; private set; } = default!;
        public bool HasUnlimitedCredit { get; private set; }

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            // Get user.
            var (user, userSharedInfo) = await userService.FindUserAsync(await ethernaOidcClient.GetEtherAddressAsync());

            EthereumAddress = userSharedInfo.EtherAddress;
            CreditBalance = await userService.GetUserBalanceAsync(user);
            HasUnlimitedCredit = user.HasUnlimitedCredit;

            return Page();
        }
    }
}
