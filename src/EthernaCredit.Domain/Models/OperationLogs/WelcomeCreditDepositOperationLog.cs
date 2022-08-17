namespace Etherna.CreditSystem.Domain.Models.OperationLogs
{
    public class WelcomeCreditDepositOperationLog : OperationLogBase
    {
        // Constructors.
        public WelcomeCreditDepositOperationLog(
            decimal amount,
            string author,
            User user)
            : base(amount, author, user)
        { }
        protected WelcomeCreditDepositOperationLog() { }

        // Properties.
        public override string OperationName => "Welcome Credit Deposit";
    }
}
