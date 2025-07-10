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
using Etherna.Credit.Domain.Models.UserAgg;
using Etherna.Credit.Persistence.Repositories;
using Etherna.MongoDB.Driver;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Repositories;
using Etherna.MongODM.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Etherna.Credit.Persistence
{
    public class SharedDbContext : DbContext, ISharedDbContext
    {
        // Consts.
        private const string ModelMapsNamespace = "Etherna.Credit.Persistence.ModelMaps.Shared";

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
