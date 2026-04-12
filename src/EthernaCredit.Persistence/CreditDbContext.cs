// Copyright 2021-present Etherna SA
// This file is part of Etherna Credit.
// 
// Etherna Credit is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Credit is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Credit.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.Credit.Domain;
using Etherna.Credit.Domain.Models;
using Etherna.Credit.Domain.Models.UserAgg;
using Etherna.Credit.Persistence.Repositories;
using Etherna.DomainEvents;
using Etherna.MongoDB.Driver;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Repositories;
using Etherna.MongODM.Core.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.Credit.Persistence
{
    public class CreditDbContext(
        IEventDispatcher eventDispatcher,
        ILogger<CreditDbContext> logger)
        : DbContext(logger), ICreditDbContextInternal, IEventDispatcherDbContext
    {
        // Consts.
        private const string SerializersNamespace = "Etherna.Credit.Persistence.ModelMaps.Credit";

        // Properties.
        //repositories
        public IRepository<OperationLogBase, string> OperationLogs { get; } =
            new DomainRepository<OperationLogBase, string>("logs");
        public IRepository<ProcessedCryptoTransaction, string> ProcessedCryptoTransactions { get; } =
            new DomainRepository<ProcessedCryptoTransaction, string>(
                new RepositoryOptions<ProcessedCryptoTransaction>("processedCryptoTransactions")
                {
                    IndexBuilders =
                    [
                        (Builders<ProcessedCryptoTransaction>.IndexKeys.Ascending(t => t.TxId), new CreateIndexOptions<ProcessedCryptoTransaction> { Unique = true })
                    ]
                });

        public IRepository<UserCryptoWallet, string> UserCryptoWallets { get; } =
            new DomainRepository<UserCryptoWallet, string>(
                new RepositoryOptions<UserCryptoWallet>("userCryptoWallets")
                {
                    IndexBuilders =
                    [
                        (Builders<UserCryptoWallet>.IndexKeys.Ascending(w => w.Author.Id).Ascending(w => w.Symbol), new CreateIndexOptions<UserCryptoWallet> { Unique = true }),
                        (Builders<UserCryptoWallet>.IndexKeys.Ascending(w => w.Wallet), new CreateIndexOptions<UserCryptoWallet> { Unique = true })
                    ]
                });
        public IRepository<User, string> Users { get; } = new DomainRepository<User, string>(
            new RepositoryOptions<User>("users")
            {
                IndexBuilders =
                [
                    (Builders<User>.IndexKeys.Ascending(u => u.SharedInfoId), new CreateIndexOptions<User> { Unique = true })
                ]
            });

        //internal repositories
        public IRepository<UserBalance, string> UserBalances { get; } = new DomainRepository<UserBalance, string>(
            new RepositoryOptions<UserBalance>("userBalances")
            {
                IndexBuilders =
                [
                    (Builders<UserBalance>.IndexKeys.Ascending(b => b.User.Id), new CreateIndexOptions<UserBalance> { Unique = true })
                ]
            });

        //other properties
        public IEventDispatcher EventDispatcher { get; } = eventDispatcher;

        // Protected properties.
        protected override IEnumerable<IModelMapsCollector> ModelMapsCollectors =>
            from t in typeof(CreditDbContext).GetTypeInfo().Assembly.GetTypes()
            where t.IsClass && t.Namespace == SerializersNamespace
            where t.GetInterfaces().Contains(typeof(IModelMapsCollector))
            select Activator.CreateInstance(t) as IModelMapsCollector;

        // Methods.
        public override async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var changedEntityModels = ChangedModelsList.OfType<EntityModelBase>().ToArray();

            // Save changes.
            await base.SaveChangesAsync(cancellationToken);

            // Dispatch events.
            foreach (var model in changedEntityModels)
            {
                await EventDispatcher.DispatchAsync(model.Events);
                model.ClearEvents();
            }
        }
    }
}
