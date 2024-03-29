﻿//   Copyright 2021-present Etherna Sa
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

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
