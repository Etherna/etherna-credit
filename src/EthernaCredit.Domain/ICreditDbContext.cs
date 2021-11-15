using Etherna.DomainEvents;
using Etherna.CreditSystem.Domain.Models;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Repositories;

namespace Etherna.CreditSystem.Domain
{
    public interface ICreditDbContext : IDbContext
    {
        ICollectionRepository<OperationLogBase, string> OperationLogs { get; }
        ICollectionRepository<User, string> Users { get; }

        IEventDispatcher EventDispatcher { get; }
    }
}
