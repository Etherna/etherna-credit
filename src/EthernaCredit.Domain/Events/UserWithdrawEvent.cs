using Etherna.CreditSystem.Domain.Models;
using Etherna.DomainEvents;

namespace Etherna.CreditSystem.Domain.Events
{
    public class UserWithdrawEvent : IDomainEvent
    {
        // Constructor.
        public UserWithdrawEvent(decimal ammount, User user)
        {
            Ammount = ammount;
            User = user;
        }

        // Properties.
        public decimal Ammount { get; }
        public User User { get; }
    }
}
