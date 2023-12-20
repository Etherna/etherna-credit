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

using Etherna.Authentication;
using Etherna.CreditSystem.Services.Domain;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Configs.Authorization
{
    public class DenyBannedAuthorizationHandler : AuthorizationHandler<DenyBannedAuthorizationRequirement>
    {
        // Fields.
        private readonly IEthernaOpenIdConnectClient ethernaOidcClient;
        private readonly IUserService userService;

        // Constructor
        public DenyBannedAuthorizationHandler(
            IEthernaOpenIdConnectClient ethernaOidcClient,
            IUserService userService)
        {
            this.ethernaOidcClient = ethernaOidcClient;
            this.userService = userService;
        }

        // Methods.
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DenyBannedAuthorizationRequirement requirement)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var sharedInfo = await userService.TryFindUserSharedInfoByAddressAsync(await ethernaOidcClient.GetEtherAddressAsync());
                if (sharedInfo is null)
                {
                    context.Fail();
                    return;
                }

                if (sharedInfo.IsLockedOutNow)
                    context.Fail();
                else
                    context.Succeed(requirement);
            }
        }
    }
}
