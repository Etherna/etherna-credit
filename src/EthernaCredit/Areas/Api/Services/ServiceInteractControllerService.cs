using Etherna.CreditSystem.Areas.Api.DtoModels;
using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.OperationLogs;
using Etherna.CreditSystem.Services.Domain;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Driver;
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
        public async Task<CreditDto> GetUserCreditAsync(string address)
        {
            var (user, _) = await userService.TryFindUserAsync(address);
            if (user is null)
                return new CreditDto(0, false);

            var balance = await userService.GetUserBalanceAsync(user);

            return new CreditDto(balance, user.HasUnlimitedCredit);
        }

        public async Task RegisterBalanceUpdateAsync(string clientId, string address, decimal amount, string reason)
        {
            // Get user.
            var (user, _) = await userService.FindUserAsync(address);

            // Apply update (balance can go negative).
            var result = await userService.IncrementUserBalanceAsync(user, amount, true);
            if (!result)
                throw new InvalidOperationException();

            // Create or update log.
            var updatedLog = await dbContext.OperationLogs.AccessToCollectionAsync(collection =>
                collection.FindOneAndUpdateAsync(
                    Builders<OperationLogBase>.Filter.OfType<UpdateOperationLog>(
                        log => log.Author == clientId &&
                               log.CreationDateTime >= DateTime.Now.Date &&
                               log.Reason == reason &&
                               log.User.Id == user.Id),
                    Builders<OperationLogBase>.Update.Inc(log => log.Amount, new Decimal128(amount))));

            if (updatedLog is null) //if a previous log didn't exist
            {
                var withdrawLog = new UpdateOperationLog(amount, clientId, reason, user);
                await dbContext.OperationLogs.CreateAsync(withdrawLog);
            }
        }
    }
}
