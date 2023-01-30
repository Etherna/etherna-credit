//   Copyright 2021-present Etherna Sagl
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
        public IRepository<UserSharedInfo, string> UsersInfo { get; } = new DomainRepository<UserSharedInfo, string>(
            new RepositoryOptions<UserSharedInfo>("usersInfo")
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
