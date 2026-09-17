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

using Etherna.Credit.Domain.Models;
using Etherna.Credit.Domain.Models.UserAgg;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.Scrinium.Core;
using Etherna.Scrinium.Core.Extensions;
using Etherna.Scrinium.Core.Serialization;
using Etherna.Scrinium.Core.Serialization.Serializers;

namespace Etherna.Credit.Persistence.ModelMaps.Credit
{
    internal sealed class UserMap : IModelMapsCollector
    {
        public void Register(IDbContextEngine dbContextEngine)
        {
            dbContextEngine.MapRegistry.AddModelMap<User>("0ff83163-b49f-4182-895d-bed59e73a976"); //dev (pre v0.3.0), published for WAM event
            dbContextEngine.MapRegistry.AddModelMap<UserBalance>("873c5ee4-122b-4021-8dc9-524b9f50b73b", //dev (pre v0.3.0), published for WAM event
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(b => b.User, ReferenceSerializer(dbContextEngine));
                });
        }

        /// <summary>
        /// The document reference with only Id.
        /// Users are never deleted, so no origin delete policy is declared.
        /// </summary>
        public static ReferenceSerializer<User, string> ReferenceSerializer(
            IDbContextEngine dbContextEngine) =>
            new(dbContextEngine, config =>
            {
                config.AddModelMap<ModelBase>("485440e2-ce18-4c40-a8ca-31280fbb22ed");
                config.AddModelMap<EntityModelBase>("bac86f72-a3d9-4ccc-bb43-9af68d6d5c03", mm => { });
                config.AddModelMap<EntityModelBase<string>>("f1cc0691-f879-4036-a994-fe92b6d39cfb", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMap<User>("b309c982-f30f-46ad-b076-c6030c8dbcd8", mm => { });
            });
    }
}
