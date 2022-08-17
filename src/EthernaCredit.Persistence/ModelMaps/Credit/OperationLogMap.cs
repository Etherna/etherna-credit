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

using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.OperationLogs;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.CreditSystem.Persistence.ModelMaps.Credit
{
    class OperationLogMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.SchemaRegistry.AddModelMapsSchema<OperationLogBase>("780ae0cc-070a-4099-a4c0-0494304e4093",
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(l => l.Amount, new Decimal128Serializer(BsonType.Decimal128));
                    mm.SetMemberSerializer(l => l.User, UserMap.ReferenceSerializer(dbContext));
                });
            dbContext.SchemaRegistry.AddModelMapsSchema<DepositOperationLog>("7fc7abe8-9a55-40a2-90ce-f3cba34bc005");
            dbContext.SchemaRegistry.AddModelMapsSchema<UpdateOperationLog>("74e021d4-6d86-4deb-b952-0c328839cfe2");
            dbContext.SchemaRegistry.AddModelMapsSchema<WelcomeCreditDepositOperationLog>("ba82b71f-1d41-45e2-a56b-d3293ea74c3a");
            dbContext.SchemaRegistry.AddModelMapsSchema<WithdrawOperationLog>("b0ffe059-c985-4f3d-8677-238ab9551ec3");
        }
    }
}
