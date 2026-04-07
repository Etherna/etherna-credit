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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.Credit.Shkeeper
{
    public class ShKeeperService : IShKeeperService, IDisposable
    {
        // Consts.
        private readonly TimeSpan AvailableCryptoFetchTTL = TimeSpan.FromSeconds(30);
        
        // Fields.
        private readonly SemaphoreSlim availableCryptosSemaphore = new(1, 1);
        private volatile IReadOnlyDictionary<string, PaymentCrypto> availableCryptos =
            new Dictionary<string, PaymentCrypto>();
        private readonly ShKeeperGeneratedClient generatedClient;
        private readonly HttpClient httpClient;
        private DateTimeOffset lastAvailableCryptoFetchTime = DateTimeOffset.MinValue;

        private bool disposed;
        
        // Constructor.
        public ShKeeperService(
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
            {
                availableCryptosSemaphore.Dispose();
                httpClient.Dispose();
            }

            disposed = true;
        }

        // Properties.
        public string ApiKey { get; }
        public Uri BaseUrl { get; }
        
        // Methods.
        public async Task<ShKeeperPaymentRequestResponse> CreatePaymentRequestAsync(
            XDaiValue amount,
            string cryptoSymbol,
            string externalId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(cryptoSymbol);
            
            var cryptoSymbolEnum = Enum.Parse<Crypto_name>(
                cryptoSymbol.Replace("-", "", StringComparison.InvariantCulture));
            
            var result = await generatedClient.ApiV1PaymentRequestAsync(
                ApiKey,
                cryptoSymbolEnum,
                new CreatePaymentRequest
                {
                    Amount = amount.ToString(),
                    Callback_url = "https://test.com/sas",
                    External_id = externalId,
                    Fiat = CreatePaymentRequestFiat.USD
                },
                cancellationToken);
            return new ShKeeperPaymentRequestResponse
            {
                Id = result.Id,
                CryptoAmount = double.Parse(result.Amount, CultureInfo.InvariantCulture),
                DisplayName = result.Display_name,
                ExchangeRate = double.Parse(result.Exchange_rate, CultureInfo.InvariantCulture),
                RecalculateAfter = TimeSpan.FromHours(result.Recalculate_after),
                Status = result.Status,
                Wallet = result.Wallet
            };
        }
        
        public async Task<IReadOnlyDictionary<string, PaymentCrypto>> GetAvailableCryptosAsync(CancellationToken cancellationToken = default)
        {
            // Try to return from cache.
            if (lastAvailableCryptoFetchTime + AvailableCryptoFetchTTL >= DateTimeOffset.Now)
                return availableCryptos;

            // Else, fetch from ShKeeper with atomic refresh.
            await availableCryptosSemaphore.WaitAsync(cancellationToken);
            try
            {
                // Double-check after acquiring the semaphore.
                if (lastAvailableCryptoFetchTime + AvailableCryptoFetchTTL >= DateTimeOffset.Now)
                    return availableCryptos;

                var result = await generatedClient.ApiV1CryptoAsync(cancellationToken);
                availableCryptos = result.Crypto_list
                    .ToDictionary(
                        c => c.Name,
                        c => new PaymentCrypto(c.Display_name, c.Name));
                lastAvailableCryptoFetchTime = DateTimeOffset.Now;

                return availableCryptos;
            }
            finally
            {
                availableCryptosSemaphore.Release();
            }
        }
    }
}