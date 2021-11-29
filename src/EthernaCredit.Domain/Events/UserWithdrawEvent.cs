using Etherna.CreditSystem.Domain.Models;
using Etherna.DomainEvents;

namespace Etherna.CreditSystem.Domain.Events
{
    public class UserWithdrawEvent : IDomainEvent
    {
        // Constructor.
        public UserWithdrawEvent(decimal amount, User user)
        {
            Amount = amount;
            User = user;
        }

        // Properties.
        public decimal Amount { get; }
        public User User { get; }
    }
}
