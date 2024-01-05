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
