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
         * pointing to SharedInfo internal property, and avoid data duplication.
         */
        //protected virtual SharedUserInfo SharedInfo { get; set; }
        public virtual string SharedInfoId { get; protected set; } = default!;
    }
}
