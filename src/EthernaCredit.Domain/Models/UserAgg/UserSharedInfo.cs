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

using System;
using System.Collections.Generic;

namespace Etherna.CreditSystem.Domain.Models.UserAgg
{
    /* This model will not be encapsulated with User until https://etherna.atlassian.net/browse/MODM-101 is solved.
     * After it, a full referenced inclusion can be implemented. */

    /// <summary>
    /// Shared with other Etherna services. Use in read-only mode.
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
