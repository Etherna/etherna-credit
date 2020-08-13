using Etherna.DomainEvents;
using Etherna.EthernaCredit.Domain;
using Etherna.EthernaCredit.Domain.Models;
using Etherna.EthernaCredit.Persistence.Repositories;
using Etherna.MongODM;
using Etherna.MongODM.Repositories;
using Etherna.MongODM.Serialization;
using Etherna.MongODM.Utility;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.EthernaCredit.Persistence
{
    public class CreditContext : DbContext, ICreditContext, IEventDispatcherDbContext
    {
        // Consts.
        private const string SerializersNamespace = "Etherna.EthernaCredit.Persistence.ModelMaps";

        // Constructor.
        public CreditContext(
            IDbDependencies dbDependencies,
            IEventDispatcher eventDispatcher,
            DbContextOptions<CreditContext> options)
            : base(dbDependencies, options)
        {
            EventDispatcher = eventDispatcher;
        }

        // Properties.
        //repositories
        public ICollectionRepository<OperationLogBase, string> OperationLogs { get; } = new DomainCollectionRepository<OperationLogBase, string>(
            new CollectionRepositoryOptions<OperationLogBase>("logs")
            {
                IndexBuilders = new[]
                {
                    (Builders<OperationLogBase>.IndexKeys.Ascending(l => l.User.Address), new CreateIndexOptions<OperationLogBase>()),
                }
            });
        public ICollectionRepository<User, string> Users { get; } = new DomainCollectionRepository<User, string>(
            new CollectionRepositoryOptions<User>("users")
            {
                IndexBuilders = new[]
                {
                    (Builders<User>.IndexKeys.Ascending(u => u.Address), new CreateIndexOptions<User> { Unique = true }),
                }
            });

        //other properties
        public IEventDispatcher EventDispatcher { get; }

        // Protected properties.
        protected override IEnumerable<IModelMapsCollector> ModelMapsCollectors =>
            from t in typeof(CreditContext).GetTypeInfo().Assembly.GetTypes()
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
