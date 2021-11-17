using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Services.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Manage.Pages
{
    public class LogsModel : PageModel
    {
        // Consts.
        public const int DefaultTakeElements = 20;

        // Fields.
        private readonly ICreditDbContext dbContext;
        private readonly IUserService userService;

        // Constructor.
        public LogsModel(
            ICreditDbContext dbContext,
            IUserService userService)
        {
            this.dbContext = dbContext;
            this.userService = userService;
        }

        // Properties.
        public int CurrentPage { get; private set; }
        public int MaxPage { get; private set; }
        public IEnumerable<OperationLogBase> Logs { get; private set; } = Array.Empty<OperationLogBase>();

        // Methods.
        public async Task OnGetAsync(int p)
        {
            // Get user.
            var user = await userService.FindAndUpdateUserAsync(User);

            // Get paginated logs.
            var paginatedLogs = await dbContext.OperationLogs.QueryPaginatedElementsAsync(
                elements => elements.Where(l => l.User.Id == user.Id),
                l => l.CreationDateTime,
                p, DefaultTakeElements);

            CurrentPage = paginatedLogs.CurrentPage;
            MaxPage = paginatedLogs.MaxPage;
            Logs = paginatedLogs.Elements;
        }
    }
}
