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

using System;
using System.Net.Http;

namespace Etherna.Credit.Shkeeper
{
    public class SHKeeperClient : ISHKeeperClient, IDisposable
    {
        // Fields.
        private readonly SHKeeperGeneratedClient generatedClient;
        private readonly HttpClient httpClient;
        
        private bool disposed;
        
        // Constructor.
        public SHKeeperClient(
            Uri baseUrl,
            HttpClient? httpClient = null)
        {
            ArgumentNullException.ThrowIfNull(baseUrl);
            
            this.httpClient = httpClient ?? new HttpClient();

            generatedClient = new SHKeeperGeneratedClient(this.httpClient) { BaseUrl = baseUrl.ToString() };
            BaseUrl = baseUrl;
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
        public Uri BaseUrl { get; }
    }
}