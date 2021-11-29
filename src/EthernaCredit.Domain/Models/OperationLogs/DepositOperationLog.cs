namespace Etherna.CreditSystem.Domain.Models.OperationLogs
{
    public class DepositOperationLog : OperationLogBase
    {
        // Constructors.
        public DepositOperationLog(
            decimal amount,
            string author,
            User user)
            : base(amount, author, user)
        { }
        protected DepositOperationLog() { }

        // Properties.
        public override string OperationName => "Deposit";
    }
}
