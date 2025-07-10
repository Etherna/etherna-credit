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
using Etherna.Credit.Domain.Models;
using Etherna.Credit.Services.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.Credit.Areas.Manage.Pages
{
    public class LogsModel : PageModel
    {
        // Consts.
        public const int DefaultTakeElements = 10;

        // Fields.
        private readonly ICreditDbContext dbContext;
        private readonly IEthernaOpenIdConnectClient ethernaOidcClient;
        private readonly IUserService userService;

        // Constructor.
        public LogsModel(
            ICreditDbContext dbContext,
            IEthernaOpenIdConnectClient ethernaOidcClient,
            IUserService userService)
        {
            this.dbContext = dbContext;
            this.ethernaOidcClient = ethernaOidcClient;
            this.userService = userService;
        }

        // Properties.
        public int CurrentPage { get; private set; }
        public long MaxPage { get; private set; }
        public IEnumerable<OperationLogBase> Logs { get; private set; } = Array.Empty<OperationLogBase>();

        // Methods.
        public async Task OnGetAsync(int p = 1)
        {
            // Get user.
            var (user, _) = await userService.FindUserAsync(await ethernaOidcClient.GetEtherAddressAsync());

            // Get paginated logs.
            var paginatedLogs = await dbContext.OperationLogs.QueryPaginatedElementsAsync(
                elements => elements.Where(l => l.User.Id == user.Id),
                l => l.CreationDateTime,
                p - 1, DefaultTakeElements,
                true);

            CurrentPage = paginatedLogs.CurrentPage + 1;
            MaxPage = paginatedLogs.MaxPage + 1;
            Logs = paginatedLogs.Elements;
        }
    }
}
