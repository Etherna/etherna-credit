using Etherna.EthernaCredit.Domain;
using Etherna.EthernaCredit.Domain.Models;
using Etherna.EthernaCredit.Domain.Models.OperationLogs;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Threading.Tasks;

namespace Etherna.EthernaCredit.Areas.Api.Services
{
    public class ServiceInteractControllerService : IServiceInteractControllerService
    {
        // Fields.
        private readonly ICreditContext creditContext;

        // Constructor.
        public ServiceInteractControllerService(ICreditContext creditContext)
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

        public async Task RegisterBalanceUpdateAsync(string address, double ammount, string reason)
        {
            var clientName = "currentClient"; //get from authentication claims

            // Apply update.
            var user = await creditContext.Users.Collection.FindOneAndUpdateAsync(
                u => u.Address == address &&
                     u.CreditBalance >= -ammount, //verify disponibility
                Builders<User>.Update.Inc(u => u.CreditBalance, ammount));

            // Verify result.
            if (user is null) //doesn't exists or unavailable ammount for debit
                throw new InvalidOperationException();

            // Report log.
            var withdrawLog = new UpdateOperationLog(ammount, clientName, reason, user);
            await creditContext.OperationLogs.CreateAsync(withdrawLog);
        }
    }
}
