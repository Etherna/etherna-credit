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

namespace Etherna.CreditSystem.Configs
{
    public static class CommonConsts
    {
        public const string AccountArea = "Account";
        public const string AdminArea = "Admin";
        public const string ApiArea = "Api";
        public const string DepositArea = "Deposit";
        public const string ManageArea = "Manage";
        public const string WithdrawArea = "Withdraw";

        public const string DatabaseAdminPath = "/admin/db";
        public const string HangfireAdminPath = "/admin/hangfire";

        public const string RequireAdministratorClaimPolicy = "RequireAdministratorClaimPolicy";
        public const string ServiceInteractApiScopePolicy = "ServiceInteractApiScope";

        public const string AdministratorRoleName = "ADMINISTRATOR";

        public const string UserAuthenticationPolicyScheme = "userAuthnPolicyScheme";
        public const string UserAuthenticationCookieScheme = "userAuthnCookieScheme";
        public const string UserAuthenticationJwtScheme = "userAuthnJwtScheme";
        public const string ServiceAuthenticationScheme = "serviceAuthnScheme";

        public const string SharedCookieApplicationName = "ethernaSharedCookie";
    }
}
