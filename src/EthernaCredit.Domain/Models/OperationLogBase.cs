using Etherna.MongoDB.Bson;
using System;

namespace Etherna.CreditSystem.Domain.Models
{
    public abstract class OperationLogBase : EntityModelBase<string>
    {
        // Constructors.
        protected OperationLogBase(
            decimal amount,
            string author,
            User user)
        {
            Amount = amount;
            Author = author ?? throw new ArgumentNullException(nameof(author));
            User = user ?? throw new ArgumentNullException(nameof(user));
        }
        protected OperationLogBase() { }

        // Properties.
        public virtual Decimal128 Amount { get; protected set; }
        public virtual string Author { get; protected set; } = default!;
        public abstract string OperationName { get; }
        public virtual User User { get; protected set; } = default!;
    }
}
