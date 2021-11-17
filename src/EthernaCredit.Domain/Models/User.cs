using Nethereum.Util;
using System;

namespace Etherna.CreditSystem.Domain.Models
{
    public class User : EntityModelBase<string>
    {
        // Constructors.
        public User(string address)
        {
            SetAddressHelper(address);
        }
        protected User() { }

        // Properties.
        public virtual string Address { get; protected set; } = default!;
        public virtual double CreditBalance { get; protected set; }

        // Methods.
        public virtual void UpdateAddress(string address) =>
            SetAddressHelper(address);

        // Methods.
        private void SetAddressHelper(string address)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));
            if (!address.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("The value is not a valid address", nameof(address));

            Address = address.ConvertToEthereumChecksumAddress();
        }
    }
}
