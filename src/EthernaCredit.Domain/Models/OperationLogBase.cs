using System;

namespace Etherna.CreditSystem.Domain.Models
{
    public abstract class OperationLogBase : EntityModelBase<string>
    {
        // Constructors.
        protected OperationLogBase(
            decimal ammount,
            string author,
            User user)
        {
            Ammount = ammount;
            Author = author ?? throw new ArgumentNullException(nameof(author));
            User = user ?? throw new ArgumentNullException(nameof(user));
        }
        protected OperationLogBase() { }

        // Properties.
        public virtual decimal Ammount { get; protected set; }
        public virtual string Author { get; protected set; } = default!;
        public abstract string OperationName { get; }
        public virtual User User { get; protected set; } = default!;
    }
}
