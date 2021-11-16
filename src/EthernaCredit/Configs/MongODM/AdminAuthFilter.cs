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

            var result = await authorizationService.AuthorizeAsync(context.User, CommonConsts.RequireAdministratorClaimPolicy);
            return result.Succeeded;
        }
    }
}
