//   Copyright 2021-present Etherna Sa
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

using System;

namespace Etherna.CreditSystem.Domain.Models.OperationLogs
{
    public class UpdateOperationLog : OperationLogBase
    {
        // Constructors.
        public UpdateOperationLog(
            XDaiBalance amount,
            string author,
            string reason,
            User user)
            : base(amount, author, user)
        {
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }
        protected UpdateOperationLog() { }

        // Properties.
        public override string OperationName => "Service update";
        public virtual string Reason { get; protected set; } = default!;
    }
}
