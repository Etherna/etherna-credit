using Etherna.CreditSystem.Domain.Models;
using Etherna.DomainEvents;

namespace Etherna.CreditSystem.Domain.Events
{
    public class UserDepositEvent : IDomainEvent
    {
        // Constructor.
        public UserDepositEvent(decimal amount, User user)
        {
            Amount = amount;
            User = user;
        }

        // Properties.
        public decimal Amount { get; }
        public User User { get; }
    }
}
