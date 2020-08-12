using Etherna.DomainEvents;
using Etherna.MongODM.Models;

namespace Etherna.EthernaCredit.Domain.Events
{
    public class EntityDeletedEvent<TModel> : IDomainEvent
        where TModel : IEntityModel
    {
        public EntityDeletedEvent(TModel entity)
        {
            Entity = entity;
        }

        public TModel Entity { get; }
    }
}
