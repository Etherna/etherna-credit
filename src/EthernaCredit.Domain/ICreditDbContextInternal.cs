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

using Etherna.Credit.Domain.Models.UserAgg;
using Etherna.MongODM.Core.Repositories;

namespace Etherna.Credit.Domain
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
