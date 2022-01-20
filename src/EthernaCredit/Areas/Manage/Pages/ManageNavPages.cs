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
