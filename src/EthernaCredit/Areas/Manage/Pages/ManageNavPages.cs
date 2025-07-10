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

using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace Etherna.CreditSystem.Areas.Manage.Pages
{
    internal static class ManageNavPages
    {
        // Properties.
        public static string Status => "Status";
        public static string Logs => "Logs";
        public static string Deposit => "Deposit";
        public static string Withdraw => "Withdraw";

        // Methods.
        public static string? StatusNavClass(ViewContext viewContext)
        {
            ArgumentNullException.ThrowIfNull(viewContext, nameof(viewContext));

            return PageNavClass(viewContext, Status);
        }

        public static string? LogsNavClass(ViewContext viewContext)
        {
            ArgumentNullException.ThrowIfNull(viewContext, nameof(viewContext));

            return PageNavClass(viewContext, Logs);
        }

        public static string? DepositNavClass(ViewContext viewContext)
        {
            ArgumentNullException.ThrowIfNull(viewContext, nameof(viewContext));

            return PageNavClass(viewContext, Deposit);
        }

        public static string? WithdrawNavClass(ViewContext viewContext)
        {
            ArgumentNullException.ThrowIfNull(viewContext, nameof(viewContext));

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
