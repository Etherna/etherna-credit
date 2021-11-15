using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.OperationLogs;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Api.Services
{
    public class ServiceInteractControllerService : IServiceInteractControllerService
    {
        // Fields.
        private readonly ICreditDbContext creditContext;

        // Constructor.
        public ServiceInteractControllerService(ICreditDbContext creditContext)
        {
            this.creditContext = creditContext;
        }

        // Methods.
        public async Task<double> GetUserBalanceAsync(string address)
        {
            var user = await creditContext.Users.QueryElementsAsync(elements =>
                elements.Where(u => u.Address == address)
                        .FirstOrDefaultAsync());

            return user?.CreditBalance ?? 0;
        }

        public async Task RegisterBalanceUpdateAsync(string clientId, string address, double ammount, string reason)
        {
            // Apply update.
            var user = await creditContext.Users.Collection.FindOneAndUpdateAsync(
                u => u.Address == address,
                Builders<User>.Update.Inc(u => u.CreditBalance, ammount));

            // Verify result.
            if (user is null)
                throw new InvalidOperationException();

            // Report log.
            var withdrawLog = new UpdateOperationLog(ammount, clientId, reason, user);
            await creditContext.OperationLogs.CreateAsync(withdrawLog);
        }
    }
}
