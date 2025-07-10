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
using Etherna.Credit.Domain;
using Etherna.Credit.Domain.Events;
using Etherna.Credit.Domain.Models;
using Etherna.Credit.Domain.Models.OperationLogs;
using Etherna.Credit.Services.Domain;
using Etherna.DomainEvents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.Credit.Areas.Admin.Pages.Users
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
            await InitializeAsync(id);
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (!ModelState.IsValid)
            {
                await InitializeAsync(id);
                return Page();
            }
            
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
        
        // Helpers.
        private async Task InitializeAsync(string id)
        {
            var user = await creditDbContext.Users.FindOneAsync(id);
            
            Id = id;
            CurrentBalance = await userService.GetUserBalanceAsync(user);
        }
    }
}