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

using Etherna.MongODM.Core.Attributes;
using System;
using System.Collections.Generic;

namespace Etherna.Credit.Domain.Models
{
    public class UserCryptoWallet : EntityModelBase<string>
    {
        // Fields.
        private HashSet<string> _wallets = [];
        
        // Constructors.
        public UserCryptoWallet(
            User author,
            string confirmSecret,
            string symbol,
            string wallet)
        {
            Author = author;
            ConfirmSecret = confirmSecret;
            Symbol = symbol;
            _wallets.Add(wallet);
        }
        protected UserCryptoWallet() { }

        // Properties.
        public virtual User Author { get; protected set; } = null!;
        public virtual string ConfirmSecret { get; protected set; } = null!;
        public virtual string Symbol { get; protected set; } = null!;
        public virtual IEnumerable<string> Wallets
        {
            get => _wallets;
            protected set => _wallets = [..value ?? []];
        }
        
        // Methods.
        [PropertyAlterer(nameof(Wallets))]
        public bool AddWallet(string wallet) =>
            _wallets.Add(wallet);
    }
}
