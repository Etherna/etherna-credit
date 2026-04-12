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
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;
using Etherna.MongODM.Core.Serialization.Serializers;

namespace Etherna.Credit.Persistence.ModelMaps.Credit
{
    internal sealed class UserCryptoWalletMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddModelMap<UserCryptoWallet>("a8a88bd9-338a-4202-87d0-c6a5a3e312d6", //0.4.0
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(w => w.Author, UserMap.ReferenceSerializer(dbContext));
                });
        }

        /// <summary>
        /// The document reference with only Id.
        /// </summary>
        public static ReferenceSerializer<UserCryptoWallet, string> ReferenceSerializer(
            IDbContext dbContext) =>
            new(dbContext, config =>
            {
                config.AddModelMap<ModelBase>("0859384e-e96d-4e60-998c-3cf1e514a887");
                config.AddModelMap<EntityModelBase>("8dd36b47-c1c0-4aae-adc9-f0ed90a2656f", _ => { });
                config.AddModelMap<EntityModelBase<string>>("8da0fcc8-5d0e-420a-8d90-9ee688ff47fc", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMap<UserCryptoWallet>("923c1937-f7db-40ee-94b3-f390d6067a47", _ => { });
            });
    }
}
