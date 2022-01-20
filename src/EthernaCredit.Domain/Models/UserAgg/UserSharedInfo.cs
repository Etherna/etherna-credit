//   Copyright 2021-present Etherna Sagl
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
using System.Collections.Generic;

namespace Etherna.CreditSystem.Domain.Models.UserAgg
{
    /* This model will not be encapsulated with UserBase until https://etherna.atlassian.net/browse/MODM-101 is solved.
     * After it, a full referenced inclusion can be implemented. */

    /// <summary>
    /// A class that expose <see cref="UserBase"/> information, but that needs to be saved on a different DbContext,
    /// shared with other Etherna services. Use in read-only mode.
    /// </summary>
    public class UserSharedInfo : EntityModelBase<string>
    {
        // Fields.
        private List<string> _etherPreviousAddresses = new();

        // Constructors.
        protected UserSharedInfo() { }

        // Properties.
        public virtual string EtherAddress { get; protected set; } = default!;
        public virtual IEnumerable<string> EtherPreviousAddresses
        {
            get => _etherPreviousAddresses;
            protected set => _etherPreviousAddresses = new List<string>(value ?? Array.Empty<string>());
        }
        public bool IsLockedOutNow => LockoutEnabled && LockoutEnd >= DateTimeOffset.UtcNow;
        public virtual bool LockoutEnabled { get; protected set; }
        public virtual DateTimeOffset? LockoutEnd { get; protected set; }
    }
}
