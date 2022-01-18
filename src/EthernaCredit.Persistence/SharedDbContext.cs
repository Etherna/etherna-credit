using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models.UserAgg;
using Etherna.CreditSystem.Persistence.Repositories;
using Etherna.MongoDB.Driver;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Repositories;
using Etherna.MongODM.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Etherna.CreditSystem.Persistence
{
    public class SharedDbContext : DbContext, ISharedDbContext
    {
        // Consts.
        private const string ModelMapsNamespace = "Etherna.CreditSystem.Persistence.ModelMaps.Shared";

        // Properties.
        //repositories
        public ICollectionRepository<UserSharedInfo, string> UsersInfo { get; } = new DomainCollectionRepository<UserSharedInfo, string>(
            new CollectionRepositoryOptions<UserSharedInfo>("usersInfo")
            {
                IndexBuilders = new[]
                {
                    (Builders<UserSharedInfo>.IndexKeys.Ascending(u => u.EtherAddress),
                     new CreateIndexOptions<UserSharedInfo> { Unique = true }),

                    (Builders<UserSharedInfo>.IndexKeys.Ascending(u => u.EtherPreviousAddresses),
                     new CreateIndexOptions<UserSharedInfo>()),
                }
            });

        // Protected properties.
        protected override IEnumerable<IModelMapsCollector> ModelMapsCollectors =>
            from t in typeof(SharedDbContext).GetTypeInfo().Assembly.GetTypes()
            where t.IsClass && t.Namespace == ModelMapsNamespace
            where t.GetInterfaces().Contains(typeof(IModelMapsCollector))
            select Activator.CreateInstance(t) as IModelMapsCollector;
    }
}
