using Etherna.Authentication.Extensions;
using Etherna.CreditSystem.Services.Domain;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Configs.Authorization
{
    public class DenyBannedAuthorizationHandler : AuthorizationHandler<DenyBannedAuthorizationRequirement>
    {
        // Fields.
        private readonly IUserService userService;

        // Constructor
        public DenyBannedAuthorizationHandler(
            IUserService userService)
        {
            this.userService = userService;
        }

        // Methods.
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DenyBannedAuthorizationRequirement requirement)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var sharedInfo = await userService.TryFindUserSharedInfoByAddressAsync(context.User.GetEtherAddress());
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
