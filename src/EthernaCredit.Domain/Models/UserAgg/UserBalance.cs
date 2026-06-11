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

namespace Etherna.Credit.Domain.Models.UserAgg
{
    /// <summary>
    /// This class is unmanaged from domain space.
    /// It needs atomic direct operations on db, interact only with IUserService.
    /// </summary>
    public class UserBalance : EntityModelBase<string>
    {
        // Constructors.
        public UserBalance(User user, XDaiValue welcomeCredit)
        {
            Credit = welcomeCredit;
            User = user;
        }
        protected UserBalance() { }

        // Properties.
        public virtual XDaiValue Credit { get; protected set; }
        public virtual User User { get; protected set; } = default!;
    }
}
