namespace Etherna.EthernaCredit.Domain.Models.OperationLogs
{
    public class DepositOperationLog : OperationLogBase
    {
        // Constructors.
        public DepositOperationLog(
            double ammount,
            string author,
            User user)
            : base(ammount, author, user)
        { }
        protected DepositOperationLog() { }
    }
}
