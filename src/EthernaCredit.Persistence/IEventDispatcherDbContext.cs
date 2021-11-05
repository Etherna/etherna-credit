using Etherna.DomainEvents;
using Etherna.MongODM.Core;

namespace Etherna.EthernaCredit.Persistence
{
    public interface IEventDispatcherDbContext : IDbContext
    {
        IEventDispatcher EventDispatcher { get; }
    }
}
