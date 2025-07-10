// Copyright 2021-present Etherna SA
// This file is part of Etherna Credit.
// 
// Etherna Credit is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Credit is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Credit.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.OperationLogs;
using Etherna.CreditSystem.Domain.Models.UserAgg;
using System;

namespace Etherna.CreditSystem.Areas.Api.DtoModels
{
    public class OperationLogDto
    {
        public OperationLogDto(OperationLogBase operationLog, UserSharedInfo userSharedInfo)
        {
            ArgumentNullException.ThrowIfNull(operationLog, nameof(operationLog));
            ArgumentNullException.ThrowIfNull(userSharedInfo, nameof(userSharedInfo));

            Amount = operationLog.Amount.ToDecimal();
            Author = operationLog.Author;
            CreationDateTime = operationLog.CreationDateTime;
            OperationName = operationLog.OperationName;
            UserAddress = userSharedInfo.EtherAddress;

            switch (operationLog)
            {
                case UpdateOperationLog updateLog:
                    IsApplied = updateLog.IsApplied;
                    Reason = updateLog.Reason;
                    break;
            }
        }

        public decimal Amount { get; }
        public string Author { get; }
        public DateTime CreationDateTime { get; }
        public bool? IsApplied { get; }
        public string OperationName { get; }
        public string? Reason { get; }
        public string UserAddress { get; }
    }
}
