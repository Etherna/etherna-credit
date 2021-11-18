using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.OperationLogs;
using Etherna.CreditSystem.Services.Domain;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Api.Services
{
    public class ServiceInteractControllerService : IServiceInteractControllerService
    {
        // Fields.
        private readonly ICreditDbContext dbContext;
        private readonly IUserService userService;

        // Constructor.
        public ServiceInteractControllerService(
            ICreditDbContext dbContext,
            IUserService userService)
        {
            this.dbContext = dbContext;
            this.userService = userService;
        }

        // Methods.
        public Task<double> GetUserBalanceAsync(string address) =>
            userService.GetUserBalanceAsync(address);

        public async Task RegisterBalanceUpdateAsync(string clientId, string address, double ammount, string reason)
        {
            // Get user.
            var user = await userService.FindUserByAddressAsync(address);

            // Apply update (balance can go negative).
            var result = await userService.IncrementUserBalanceAsync(user, ammount, true);
            if (!result)
                throw new InvalidOperationException();

            // Report log.
            var withdrawLog = new UpdateOperationLog(ammount, clientId, reason, user);
            await dbContext.OperationLogs.CreateAsync(withdrawLog);
        }
    }
}
