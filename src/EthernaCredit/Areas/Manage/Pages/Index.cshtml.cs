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

using Etherna.Authentication;
using Etherna.CreditSystem.Services.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Manage.Pages
{
    public class IndexModel : PageModel
    {
        // Fields.
        private readonly IEthernaOpenIdConnectClient ethernaOidcClient;
        private readonly IUserService userService;

        // Constructor.
        public IndexModel(
            IEthernaOpenIdConnectClient ethernaOidcClient,
            IUserService userService)
        {
            this.ethernaOidcClient = ethernaOidcClient;
            this.userService = userService;
        }

        // Properties.
        [Display(Name = "Credit balance")]
        public decimal CreditBalance { get; private set; }
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
