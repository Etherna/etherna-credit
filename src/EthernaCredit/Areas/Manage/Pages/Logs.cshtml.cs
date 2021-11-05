using Etherna.Authentication.Extensions;
using Etherna.EthernaCredit.Domain;
using Etherna.EthernaCredit.Domain.Models;
using Etherna.MongODM.Core.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaCredit.Areas.Credit.Pages
{
    public class LogsModel : PageModel
    {
        // Consts.
        public const int DefaultTakeElements = 20;

        // Fields.
        private readonly ICreditDbContext creditContext;

        // Constructor.
        public LogsModel(ICreditDbContext creditContext)
        {
            this.creditContext = creditContext;
        }

        // Properties.
        public int CurrentPage { get; private set; }
        public int MaxPage { get; private set; }
        public IEnumerable<OperationLogBase> Logs { get; private set; } = Array.Empty<OperationLogBase>();

        // Methods.
        public async Task OnGetAsync(int p)
        {
            var address = User.GetEtherAddress();
            var totLogs = await creditContext.OperationLogs.QueryElementsAsync(elements =>
                elements.Where(l => l.User.Address == address)
                        .CountAsync());

            MaxPage = (totLogs - 1) / DefaultTakeElements;
            CurrentPage = Math.Min(p, MaxPage);

            Logs = await creditContext.OperationLogs.QueryElementsAsync(elements =>
                elements.Where(l => l.User.Address == address)
                        .PaginateDescending(l => l.CreationDateTime, CurrentPage, DefaultTakeElements)
                        .ToListAsync());
        }
    }
}
