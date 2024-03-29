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

using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.OperationLogs;
using Etherna.CreditSystem.Persistence.Serializers;
using Etherna.MongoDB.Bson;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.CreditSystem.Persistence.ModelMaps.Credit
{
    internal sealed class OperationLogMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddModelMap<OperationLogBase>("780ae0cc-070a-4099-a4c0-0494304e4093", //dev (pre v0.3.0), published for WAM event
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(l => l.Amount, new XDaiBalanceSerializer(BsonType.Decimal128));
                    mm.SetMemberSerializer(l => l.User, UserMap.ReferenceSerializer(dbContext));
                });
            dbContext.MapRegistry.AddModelMap<AdminUpdateOperationLog>("3610fd2e-5f43-490b-bcfc-435a970a59cf"); //v0.3.12
            dbContext.MapRegistry.AddModelMap<DepositOperationLog>("7fc7abe8-9a55-40a2-90ce-f3cba34bc005"); //dev (pre v0.3.0), published for WAM event
            dbContext.MapRegistry.AddModelMap<UpdateOperationLog>("74e021d4-6d86-4deb-b952-0c328839cfe2",  //dev (pre v0.3.0), published for WAM event
                mm =>
                {
                    mm.AutoMap();
                    
                    // Set default values.
                    mm.GetMemberMap(l => l.IsApplied).SetDefaultValue(true).SetIgnoreIfDefault(true);
                });
            dbContext.MapRegistry.AddModelMap<WelcomeCreditDepositOperationLog>("ba82b71f-1d41-45e2-a56b-d3293ea74c3a"); //v0.3.9
            dbContext.MapRegistry.AddModelMap<WithdrawOperationLog>("b0ffe059-c985-4f3d-8677-238ab9551ec3"); //dev (pre v0.3.0), published for WAM event
        }
    }
}
