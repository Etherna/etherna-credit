using Etherna.DomainEvents;
using Etherna.EthernaCredit.Domain.Models;
using Etherna.MongODM;
using Etherna.MongODM.Repositories;

namespace Etherna.EthernaCredit.Domain
{
    public interface ICreditDbContext : IDbContext
    {
        ICollectionRepository<OperationLogBase, string> OperationLogs { get; }
        ICollectionRepository<User, string> Users { get; }

        IEventDispatcher EventDispatcher { get; }
    }
}
