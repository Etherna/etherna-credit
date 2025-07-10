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

using Etherna.CreditSystem.Domain.Models;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization.IdGenerators;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.CreditSystem.Persistence.ModelMaps.Credit
{
    internal sealed class ModelBaseMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            // register class maps.
            dbContext.MapRegistry.AddModelMap<ModelBase>("92a5e4a2-6b0c-46b7-aac7-70380deda7b4");
            dbContext.MapRegistry.AddModelMap<EntityModelBase>("155af6aa-92d1-4d8b-b000-5b5464f84dc7");
            dbContext.MapRegistry.AddModelMap<EntityModelBase<string>>("a0fddc7f-b64d-43b0-a40d-389b3fb21b67",
                mm =>
                {
                    mm.AutoMap();

                    // Set Id representation.
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId))
                                  .SetIdGenerator(new StringObjectIdGenerator());
                });
        }
    }
}
