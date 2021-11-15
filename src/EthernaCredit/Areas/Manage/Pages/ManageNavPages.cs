using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace Etherna.CreditSystem.Areas.Manage.Pages
{
    public static class ManageNavPages
    {
        // Properties.
        public static string Status => "Status";
        public static string Logs => "Logs";
        public static string Deposit => "Deposit";
        public static string Withdraw => "Withdraw";

        // Methods.
        public static string? StatusNavClass(ViewContext viewContext)
        {
            if (viewContext is null)
                throw new ArgumentNullException(nameof(viewContext));

            return PageNavClass(viewContext, Status);
        }

        public static string? LogsNavClass(ViewContext viewContext)
        {
            if (viewContext is null)
                throw new ArgumentNullException(nameof(viewContext));

            return PageNavClass(viewContext, Logs);
        }

        public static string? DepositNavClass(ViewContext viewContext)
        {
            if (viewContext is null)
                throw new ArgumentNullException(nameof(viewContext));

            return PageNavClass(viewContext, Deposit);
        }

        public static string? WithdrawNavClass(ViewContext viewContext)
        {
            if (viewContext is null)
                throw new ArgumentNullException(nameof(viewContext));

            return PageNavClass(viewContext, Withdraw);
        }

        // Helpers.
        private static string? PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
