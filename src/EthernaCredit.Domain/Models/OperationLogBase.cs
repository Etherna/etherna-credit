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

using Etherna.MongoDB.Bson;
using System;

namespace Etherna.CreditSystem.Domain.Models
{
    public abstract class OperationLogBase : EntityModelBase<string>
    {
        // Constructors.
        protected OperationLogBase(
            XDaiBalance amount,
            string author,
            User user)
        {
            Amount = amount;
            Author = author ?? throw new ArgumentNullException(nameof(author));
            User = user ?? throw new ArgumentNullException(nameof(user));
        }
        protected OperationLogBase() { }

        // Properties.
        public virtual XDaiBalance Amount { get; protected set; }
        public virtual string Author { get; protected set; } = default!;
        public abstract string OperationName { get; }
        public virtual User User { get; protected set; } = default!;
    }
}
