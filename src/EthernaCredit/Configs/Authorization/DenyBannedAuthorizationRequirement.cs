using Microsoft.AspNetCore.Authorization;

namespace Etherna.CreditSystem.Configs.Authorization
{
    public class DenyBannedAuthorizationRequirement : IAuthorizationRequirement
    {
    }
}
