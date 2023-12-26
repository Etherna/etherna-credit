// Copyright 2021-present Etherna Sa
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
using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Events;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.OperationLogs;
using Etherna.CreditSystem.Services.Domain;
using Etherna.DomainEvents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Admin.Pages.Users
{
    public class AdminUpdateUserBalanceModel : PageModel
    {
        // Models.
        public class InputModel
        {
            [Required]
            public XDaiBalance ChangeAmount { get; set; } //TODO: Use model binder
            
            [Required]
            public string Reason { get; set; } = default!;
        }
        
        // Fields.
        private readonly ICreditDbContext creditDbContext;
        private readonly IEthernaOpenIdConnectClient ethernaOidcClient;
        private readonly IEventDispatcher eventDispatcher;
        private readonly IUserService userService;

        // Constructor.
        public AdminUpdateUserBalanceModel(
            ICreditDbContext creditDbContext,
            IEthernaOpenIdConnectClient ethernaOidcClient,
            IEventDispatcher eventDispatcher,
            IUserService userService)
        {
            this.creditDbContext = creditDbContext;
            this.ethernaOidcClient = ethernaOidcClient;
            this.eventDispatcher = eventDispatcher;
            this.userService = userService;
        }
        
        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;
        
        public XDaiBalance CurrentBalance { get; private set; }
        public string Id { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string id)
        {
            var user = await creditDbContext.Users.FindOneAsync(id);
            
            Id = id;
            CurrentBalance = await userService.GetUserBalanceAsync(user);
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (!ModelState.IsValid)
                return Page();
            
            // Get data.
            var currentUserAddress = await ethernaOidcClient.GetEtherAddressAsync();
            
            // Update balance.
            var user = await creditDbContext.Users.FindOneAsync(id);
            await userService.TryIncrementUserBalanceAsync(user, Input.ChangeAmount, true);
            
            // Report log.
            var adminUpdateLog = new AdminUpdateOperationLog(
                Input.ChangeAmount,
                currentUserAddress,
                Input.Reason,
                user);
            await creditDbContext.OperationLogs.CreateAsync(adminUpdateLog);

            // Dispatch event.
            await eventDispatcher.DispatchAsync(new AdminUpdateUserBalanceEvent(adminUpdateLog));
            
            return RedirectToPage("User", new { id }); 
        }
    }
}