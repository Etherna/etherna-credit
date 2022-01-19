using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.OperationLogs;
using Etherna.CreditSystem.Domain.Models.UserAgg;
using Etherna.MongoDB.Bson;
using System;

namespace Etherna.CreditSystem.Areas.Api.DtoModels
{
    public class OperationLogDto
    {
        public OperationLogDto(OperationLogBase operationLog, UserSharedInfo userSharedInfo)
        {
            if (operationLog is null)
                throw new ArgumentNullException(nameof(operationLog));
            if (userSharedInfo is null)
                throw new ArgumentNullException(nameof(userSharedInfo));

            Amount = Decimal128.ToDecimal(operationLog.Amount);
            Author = operationLog.Author;
            CreationDateTime = operationLog.CreationDateTime;
            OperationName = operationLog.OperationName;
            UserAddress = userSharedInfo.EtherAddress;

            switch (operationLog)
            {
                case UpdateOperationLog updateLog:
                    Reason = updateLog.Reason;
                    break;
            }
        }

        public decimal Amount { get; }
        public string Author { get; }
        public DateTime CreationDateTime { get; }
        public string OperationName { get; }
        public string? Reason { get; }
        public string UserAddress { get; }
    }
}
