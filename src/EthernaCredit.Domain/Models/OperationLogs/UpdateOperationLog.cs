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

using Etherna.SwarmSdk.Models;
using System;

namespace Etherna.Credit.Domain.Models.OperationLogs
{
    public class UpdateOperationLog : OperationLogBase
    {
        // Constructors.
        public UpdateOperationLog(
            XDaiValue amount,
            string author,
            bool isApplied,
            string reason,
            User user)
            : base(amount, author, user)
        {
            IsApplied = isApplied;
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }
        protected UpdateOperationLog() { }

        // Properties.
        public virtual bool IsApplied { get;  protected set; }
        public override string OperationName => "Service update";
        public virtual string Reason { get; protected set; } = default!;
    }
}
