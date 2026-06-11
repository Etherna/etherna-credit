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

using Etherna.BeeNet.Models;
using Etherna.Credit.Domain.Models.UserAgg;
using Etherna.Credit.Persistence.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.Credit.Persistence.ModelMaps.Shared
{
    internal sealed class UserSharedInfoMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddCustomSerializerMap<EthAddress>( //v0.4.0
                new EthAddressSerializer());

            dbContext.MapRegistry.AddModelMap<UserSharedInfo>(
                "6d0d2ee1-6aa3-42ea-9833-ac592bfc6613", //from sso v0.3.0
                mm =>
                {
                    mm.AutoMap();

                    // Set members to ignore if null or default.
                    mm.GetMemberMap(u => u.LockoutEnd).SetIgnoreIfNull(true);
                });
        }
    }
}
