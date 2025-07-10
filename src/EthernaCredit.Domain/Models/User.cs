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

using Etherna.Credit.Domain.Models.UserAgg;
using System;

namespace Etherna.Credit.Domain.Models
{
    public class User : EntityModelBase<string>
    {
        // Constructors.
        public User(UserSharedInfo sharedInfo)
        {
            ArgumentNullException.ThrowIfNull(sharedInfo, nameof(sharedInfo));

            SharedInfoId = sharedInfo.Id;
        }
        protected User() { }

        // Properties.
        //public virtual string EtherAddress => SharedInfo.EtherAddress;
        //public virtual IEnumerable<string> EtherPreviousAddresses => SharedInfo.EtherPreviousAddresses;
        public virtual bool HasUnlimitedCredit { get; set; }

        /* SharedInfo is encapsulable with resolution of https://etherna.atlassian.net/browse/MODM-101.
         * With encapsulation we can expose also EtherAddress and EtherPreviousAddresses properties
         * pointing to SharedInfo internal property.
         */
        //protected virtual SharedUserInfo SharedInfo { get; set; }
        public virtual string SharedInfoId { get; protected set; } = default!;
    }
}
