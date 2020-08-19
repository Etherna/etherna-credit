using System;

namespace Etherna.EthernaCredit.Domain.Models.OperationLogs
{
    public class UpdateOperationLog : OperationLogBase
    {
        // Constructors.
        public UpdateOperationLog(
            double ammount,
            string author,
            string reason,
            User user)
            : base(ammount, author, user)
        {
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }
        protected UpdateOperationLog() { }

        // Properties.
        public override string OperationName => "Update";
        public virtual string Reason { get; protected set; } = default!;
    }
}
