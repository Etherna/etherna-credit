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

using Etherna.Credit.Persistence.Serializers;
using Etherna.MongoDB.Bson;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;
using Etherna.SwarmSdk.Models;

namespace Etherna.Credit.Persistence.ModelMaps.Credit
{
    internal sealed class BeeNetMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddCustomSerializerMap<EthAddress>( //v0.4.0
                new EthAddressSerializer());
            
            dbContext.MapRegistry.AddCustomSerializerMap<XDaiValue>( //v0.4.0
                new XDaiValueSerializer(BsonType.Decimal128));
        }
    }
}