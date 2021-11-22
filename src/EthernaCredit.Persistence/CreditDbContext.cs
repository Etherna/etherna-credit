using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.UserAgg;
using Etherna.CreditSystem.Persistence.Repositories;
using Etherna.DomainEvents;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Repositories;
using Etherna.MongODM.Core.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Persistence
{
    public class CreditDbContext : DbContext, ICreditDbContextInternal, IEventDispatcherDbContext
    {
        // Consts.
        private const string SerializersNamespace = "Etherna.CreditSystem.Persistence.ModelMaps";

        // Constructor.
        public CreditDbContext(
            IEventDispatcher eventDispatcher)
        {
            EventDispatcher = eventDispatcher;
        }

        // Properties.
        //repositories
        public ICollectionRepository<OperationLogBase, string> OperationLogs { get; } =
            new DomainCollectionRepository<OperationLogBase, string>("logs");
        public ICollectionRepository<User, string> Users { get; } = new DomainCollectionRepository<User, string>(
            new CollectionRepositoryOptions<User>("users")
            {
                IndexBuilders = new[]
                {
                    (Builders<User>.IndexKeys.Ascending(u => u.EtherAddress), new CreateIndexOptions<User> { Unique = true }),
                    (Builders<User>.IndexKeys.Ascending(u => u.EtherPreviousAddresses), new CreateIndexOptions<User>())
                }
            });

        //internal repositories
        public ICollectionRepository<UserBalance, string> UserBalances { get; } = new DomainCollectionRepository<UserBalance, string>(
            new CollectionRepositoryOptions<UserBalance>("userBalances")
            {
                IndexBuilders = new[]
                {
                    (Builders<UserBalance>.IndexKeys.Ascending(b => b.User.Id), new CreateIndexOptions<UserBalance> { Unique = true }),
                }
            });

        //other properties
        public IEventDispatcher EventDispatcher { get; }

        // Protected properties.
        protected override IEnumerable<IModelMapsCollector> ModelMapsCollectors =>
            from t in typeof(CreditDbContext).GetTypeInfo().Assembly.GetTypes()
            where t.IsClass && t.Namespace == SerializersNamespace
            where t.GetInterfaces().Contains(typeof(IModelMapsCollector))
            select Activator.CreateInstance(t) as IModelMapsCollector;

        // Methods.
        public override Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Dispatch events.
            foreach (var model in ChangedModelsList.Where(m => m is EntityModelBase)
                                                   .Select(m => (EntityModelBase)m))
            {
                EventDispatcher.DispatchAsync(model.Events);
                model.ClearEvents();
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
