using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Etherna.CreditSystem.Configs.Hangfire
{
    public class AdminAuthFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            if (httpContext.User is null)
                return false;
            var authorizationService = httpContext.RequestServices.GetService<IAuthorizationService>()!;

            var authTask = authorizationService.AuthorizeAsync(httpContext.User, CommonConsts.RequireAdministratorClaimPolicy);
            authTask.Wait();
            var result = authTask.Result;

            return result.Succeeded;
        }
    }
}
