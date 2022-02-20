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

using Etherna.CreditSystem.Domain.Models.UserAgg;
using System;

namespace Etherna.CreditSystem.Domain.Models
{
    public class User : EntityModelBase<string>
    {
        // Constructors.
        public User(UserSharedInfo sharedInfo)
        {
            if (sharedInfo is null)
                throw new ArgumentNullException(nameof(sharedInfo));

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
