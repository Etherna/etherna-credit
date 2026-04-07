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

using Etherna.BeeNet.Models;
using Etherna.Credit.Shkeeper.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.Credit.Shkeeper
{
    public interface IShKeeperService
    {
        Task<ShKeeperPaymentRequestResponse> CreateInvoiceAsync(
            XDaiValue amount,
            string cryptoSymbol,
            string externalId,
            CancellationToken cancellationToken = default);
        
        Task<IReadOnlyDictionary<string, PaymentCrypto>> GetAvailableCryptosAsync(
            CancellationToken cancellationToken = default);
        
        Task<ShKeeperInvoice> GetInvoiceAsync(
            string externalId,
            CancellationToken cancellationToken = default);
    }
}