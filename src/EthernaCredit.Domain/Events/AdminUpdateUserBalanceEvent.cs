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

using Etherna.Credit.Domain.Models.OperationLogs;
using Etherna.DomainEvents;

namespace Etherna.Credit.Domain.Events
{
    public class AdminUpdateUserBalanceEvent(AdminUpdateOperationLog operationLog) : IDomainEvent
    {
        // Properties.
        public AdminUpdateOperationLog OperationLog { get; } = operationLog;
    }
}