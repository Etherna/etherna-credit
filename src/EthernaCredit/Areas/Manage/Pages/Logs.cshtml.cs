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
using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Services.Domain;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Manage.Pages
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
