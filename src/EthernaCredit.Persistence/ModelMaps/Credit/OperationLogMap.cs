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
using Etherna.Credit.Domain.Models.OperationLogs;
using Etherna.Credit.Persistence.Serializers;
using Etherna.MongoDB.Bson;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.Credit.Persistence.ModelMaps.Credit
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
