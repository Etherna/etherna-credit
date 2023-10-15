//   Copyright 2021-present Etherna Sa
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

using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.UserAgg;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;
using Etherna.MongODM.Core.Serialization.Serializers;

namespace Etherna.CreditSystem.Persistence.ModelMaps.Credit
{
    class UserMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddModelMap<User>("0ff83163-b49f-4182-895d-bed59e73a976");
            dbContext.MapRegistry.AddModelMap<UserBalance>("873c5ee4-122b-4021-8dc9-524b9f50b73b",
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(b => b.Credit, new Decimal128Serializer(BsonType.Decimal128));
                    mm.SetMemberSerializer(b => b.User, ReferenceSerializer(dbContext));
                });
        }

        /// <summary>
        /// The document reference with only Id.
        /// </summary>
        public static ReferenceSerializer<User, string> ReferenceSerializer(
            IDbContext dbContext) =>
            new(dbContext, config =>
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
