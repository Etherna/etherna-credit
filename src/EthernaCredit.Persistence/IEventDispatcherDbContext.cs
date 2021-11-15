using Etherna.DomainEvents;
using Etherna.MongODM.Core;

namespace Etherna.CreditSystem.Persistence
{
    public interface IEventDispatcherDbContext : IDbContext
    {
        IEventDispatcher EventDispatcher { get; }
    }
}
