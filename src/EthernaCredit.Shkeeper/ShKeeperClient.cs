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

using Etherna.Credit.Shkeeper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.Credit.Shkeeper
{
    public class ShKeeperClient : IShKeeperClient, IDisposable
    {
        // Fields.
        private readonly ShKeeperGeneratedClient generatedClient;
        private readonly HttpClient httpClient;
        
        private bool disposed;
        
        // Constructor.
        public ShKeeperClient(
            ShKeeperOptions options,
            HttpClient? httpClient = null)
        {
            ArgumentNullException.ThrowIfNull(options);

            this.httpClient = httpClient ?? new HttpClient();

            ApiKey = options.ApiKey;
            BaseUrl = new Uri(options.Url);
            generatedClient = new ShKeeperGeneratedClient(this.httpClient) { BaseUrl = BaseUrl.ToString() };
        }

        // Dispose.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            // Dispose managed resources.
            if (disposing)
                httpClient.Dispose();

            disposed = true;
        }

        // Properties.
        public string ApiKey { get; }
        public Uri BaseUrl { get; }
        
        // Methods.
        public async Task<IEnumerable<PaymentCrypto>> GetAvailableCryptosAsync(CancellationToken cancellationToken = default)
        {
            var result = await generatedClient.ApiV1CryptoAsync(cancellationToken);
            return result.Crypto_list.Select(c => new PaymentCrypto(c.Display_name, c.Name));
        }
    }
}