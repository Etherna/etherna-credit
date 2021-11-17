using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.CreditSystem.Domain.Models
{
    public class User : EntityModelBase<string>
    {
        // Fields.
        private HashSet<string> _etherPreviousAddresses = new();

        // Constructors.
        public User(string address)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));
            if (!address.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("The value is not a valid ethereum address", nameof(address));

            EtherAddress = address.ConvertToEthereumChecksumAddress();
        }
        protected User() { }

        // Properties.
        public virtual double CreditBalance { get; protected set; }
        public virtual string EtherAddress { get; protected set; } = default!;
        public virtual IEnumerable<string> EtherPreviousAddresses
        {
            get => _etherPreviousAddresses;
            protected set => _etherPreviousAddresses = new HashSet<string>(value ?? Array.Empty<string>());
        }

        // Methods.
        public virtual void UpdateAddresses(string newAddress, IEnumerable<string> newEtherPreviousAddresses)
        {
            if (newAddress is null)
                throw new ArgumentNullException(nameof(newAddress));
            if (!newAddress.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("The value is not a valid ethereum address", nameof(newAddress));

            if (newEtherPreviousAddresses is null)
                throw new ArgumentNullException(nameof(newEtherPreviousAddresses));
            foreach (var address in newEtherPreviousAddresses)
                if (!address.IsValidEthereumAddressHexFormat())
                    throw new ArgumentException("Some values are not valid ethereum addresses", nameof(newEtherPreviousAddresses));

            var mergedPrevEtherAddresses = _etherPreviousAddresses.Union(newEtherPreviousAddresses).ToHashSet();

            if (mergedPrevEtherAddresses.Contains(newAddress))
                throw new ArgumentException("Old addresses can't contain also the new one", nameof(newEtherPreviousAddresses));
            if (!mergedPrevEtherAddresses.Contains(EtherAddress))
                throw new ArgumentException("Old addresses must contain also the last old one", nameof(newEtherPreviousAddresses));

            _etherPreviousAddresses = mergedPrevEtherAddresses;
            EtherAddress = newAddress.ConvertToEthereumChecksumAddress();
        }
    }
}
