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

namespace Etherna.Credit.Configs
{
    internal static class CommonConsts
    {
        public const string AccountArea = "Account";
        public const string AdminArea = "Admin";
        public const string ApiArea = "Api";
        public const string DepositArea = "Deposit";
        public const string ManageArea = "Manage";
        public const string WithdrawArea = "Withdraw";

        public const string DatabaseAdminPath = "/admin/db";
        public const string HangfireAdminPath = "/admin/hangfire";

        public const string RequireAdministratorRolePolicy = "RequireAdministratorRolePolicy";
        public const string ServiceInteractApiScopePolicy = "ServiceInteractApiScopePolicy";
        public const string UserInteractApiScopePolicy = "UserInteractApiScopePolicy";

        public const string AdministratorRoleName = "ADMINISTRATOR";

        public const string UserAuthenticationPolicyScheme = "userAuthnPolicyScheme";
        public const string UserAuthenticationCookieScheme = "userAuthnCookieScheme";
        public const string UserAuthenticationJwtScheme = "userAuthnJwtScheme";
        public const string ServiceAuthenticationScheme = "serviceAuthnScheme";

        public const string SharedCookieApplicationName = "ethernaSharedCookie";
    }
}
