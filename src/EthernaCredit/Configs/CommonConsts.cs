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

        public const string SharedCookieApplicationName = "ethernaSharedCookie";
    }
}
