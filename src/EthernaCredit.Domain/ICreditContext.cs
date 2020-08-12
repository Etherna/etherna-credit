using Etherna.DomainEvents;
using Etherna.EthernaCredit.Domain.Models;
using Etherna.MongODM;
using Etherna.MongODM.Repositories;

namespace Etherna.EthernaCredit.Domain
{
    public interface ICreditContext : IDbContext
    {
        ICollectionRepository<User, string> Users { get; }

        IEventDispatcher EventDispatcher { get; }
    }
}
