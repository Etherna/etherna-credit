using Etherna.DomainEvents;
using Etherna.MongODM;

namespace Etherna.EthernaCredit.Persistence
{
    public interface IEventDispatcherDbContext : IDbContext
    {
        IEventDispatcher EventDispatcher { get; }
    }
}
