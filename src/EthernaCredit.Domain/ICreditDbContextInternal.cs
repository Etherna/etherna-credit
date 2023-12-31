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

using Etherna.CreditSystem.Domain.Models.UserAgg;
using Etherna.MongODM.Core.Repositories;

namespace Etherna.CreditSystem.Domain
{
    /// <summary>
    /// Don't access directly to this. 
    /// This context exposes models unmanaged from domain space.
    /// Interact only with IUserService.
    /// </summary>
    public interface ICreditDbContextInternal : ICreditDbContext
    {
        IRepository<UserBalance, string> UserBalances { get; }
    }
}
