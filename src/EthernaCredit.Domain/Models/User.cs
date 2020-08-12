using Nethereum.Util;
using System;

namespace Etherna.EthernaCredit.Domain.Models
{
    public class User : EntityModelBase<string>
    {
        // Constructors.
        public User(string address)
        {
            SetAddress(address);
        }
        protected User() { }

        // Properties.
        public virtual string Address { get; protected set; } = default!;

        // Helpers.
        private void SetAddress(string address)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));
            if (!address.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("The value is not a valid address", nameof(address));

            Address = address.ConvertToEthereumChecksumAddress();
        }
    }
}
