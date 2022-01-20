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
using Etherna.CreditSystem.Domain.Models.UserAgg;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Services.Domain
{
    public interface IUserService
    {
        Task<(User, UserSharedInfo)> FindUserAsync(string address);
        Task<(User, UserSharedInfo)> FindUserAsync(UserSharedInfo userSharedInfo);
        Task<UserSharedInfo> FindUserSharedInfoByAddressAsync(string address);
        Task<decimal> GetUserBalanceAsync(string address);
        Task<decimal> GetUserBalanceAsync(User user);
        Task<bool> IncrementUserBalanceAsync(User user, decimal amount, bool allowBalanceDecreaseNegative);
        Task<(User?, UserSharedInfo?)> TryFindUserAsync(string address);
        Task<UserSharedInfo?> TryFindUserSharedInfoByAddressAsync(string address);
    }
}