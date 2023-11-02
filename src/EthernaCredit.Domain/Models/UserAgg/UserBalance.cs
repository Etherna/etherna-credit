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

namespace Etherna.CreditSystem.Domain.Models.UserAgg
{
    /// <summary>
    /// This class is unmanaged from domain space.
    /// It needs atomic direct operations on db, interact only with IUserService.
    /// </summary>
    public class UserBalance : EntityModelBase<string>
    {
        // Constructors.
        public UserBalance(User user, XDaiBalance welcomeCredit)
        {
            Credit = welcomeCredit;
            User = user;
        }
        protected UserBalance() { }

        // Properties.
        public virtual XDaiBalance Credit { get; protected set; }
        public virtual User User { get; protected set; } = default!;
    }
}
