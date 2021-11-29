using Etherna.CreditSystem.Domain.Models;
using Etherna.DomainEvents;

namespace Etherna.CreditSystem.Domain.Events
{
    public class UserDepositEvent : IDomainEvent
    {
        // Constructor.
        public UserDepositEvent(decimal ammount, User user)
        {
            Ammount = ammount;
            User = user;
        }

        // Properties.
        public decimal Ammount { get; }
        public User User { get; }
    }
}
