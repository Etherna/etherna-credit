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

using Etherna.MongODM.AspNetCore.UI.Auth.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Configs.MongODM
{
    public class AdminAuthFilter : IDashboardAuthFilter
    {
        public async Task<bool> AuthorizeAsync(HttpContext? context)
        {
            if (context?.User is null)
                return false;
            var authorizationService = context.RequestServices.GetService<IAuthorizationService>()!;

            var result = await authorizationService.AuthorizeAsync(context.User, CommonConsts.RequireAdministratorRolePolicy);
            return result.Succeeded;
        }
    }
}
