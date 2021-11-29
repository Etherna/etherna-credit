namespace Etherna.CreditSystem.Domain.Models.OperationLogs
{
    public class WithdrawOperationLog : OperationLogBase
    {
        // Constructors.
        public WithdrawOperationLog(
            decimal ammount,
            string author,
            User user)
            : base(ammount, author, user)
        { }
        protected WithdrawOperationLog() { }

        // Properties.
        public override string OperationName => "Withdraw";
    }
}
