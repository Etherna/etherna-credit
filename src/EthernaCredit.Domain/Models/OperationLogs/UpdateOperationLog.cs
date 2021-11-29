﻿using System;

namespace Etherna.CreditSystem.Domain.Models.OperationLogs
{
    public class UpdateOperationLog : OperationLogBase
    {
        // Constructors.
        public UpdateOperationLog(
            decimal amount,
            string author,
            string reason,
            User user)
            : base(amount, author, user)
        {
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }
        protected UpdateOperationLog() { }

        // Properties.
        public override string OperationName => "Update";
        public virtual string Reason { get; protected set; } = default!;
    }
}
