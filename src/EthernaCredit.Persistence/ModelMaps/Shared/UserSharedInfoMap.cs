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

using Etherna.CreditSystem.Domain.Models.UserAgg;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.CreditSystem.Persistence.ModelMaps.Shared
{
    internal sealed class UserSharedInfoMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddModelMap<UserSharedInfo>("6d0d2ee1-6aa3-42ea-9833-ac592bfc6613", mm =>
            {
                mm.AutoMap();

                // Set members to ignore if null or default.
                mm.GetMemberMap(u => u.LockoutEnd).SetIgnoreIfNull(true);
            });
        }
    }
}
