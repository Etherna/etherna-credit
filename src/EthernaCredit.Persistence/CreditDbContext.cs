﻿//   Copyright 2021-present Etherna Sa
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.UserAgg;
using Etherna.CreditSystem.Persistence.Repositories;
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

namespace Etherna.CreditSystem.Persistence
{
    public class CreditDbContext : DbContext, ICreditDbContextInternal, IEventDispatcherDbContext
    {
        // Consts.
        private const string SerializersNamespace = "Etherna.CreditSystem.Persistence.ModelMaps.Credit";

        // Constructor.
        public CreditDbContext(
            IEventDispatcher eventDispatcher,
            ILogger<CreditDbContext> logger)
            : base(logger)
        {
            EventDispatcher = eventDispatcher;
        }

        // Properties.
        //repositories
        public IRepository<OperationLogBase, string> OperationLogs { get; } =
            new DomainRepository<OperationLogBase, string>("logs");
        public IRepository<User, string> Users { get; } = new DomainRepository<User, string>(
            new RepositoryOptions<User>("users")
            {
                IndexBuilders = new[]
                {
                    (Builders<User>.IndexKeys.Ascending(u => u.SharedInfoId), new CreateIndexOptions<User> { Unique = true })
                }
            });

        //internal repositories
        public IRepository<UserBalance, string> UserBalances { get; } = new DomainRepository<UserBalance, string>(
            new RepositoryOptions<UserBalance>("userBalances")
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
