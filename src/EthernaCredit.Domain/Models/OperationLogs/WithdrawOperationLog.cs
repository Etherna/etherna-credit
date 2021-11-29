namespace Etherna.CreditSystem.Domain.Models.OperationLogs
{
    public class WithdrawOperationLog : OperationLogBase
    {
        // Constructors.
        public WithdrawOperationLog(
            decimal amount,
            string author,
            User user)
            : base(amount, author, user)
        { }
        protected WithdrawOperationLog() { }

        // Properties.
        public override string OperationName => "Withdraw";
    }
}
