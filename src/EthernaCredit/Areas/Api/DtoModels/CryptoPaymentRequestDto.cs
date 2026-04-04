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
using System;

namespace Etherna.Credit.Areas.Api.DtoModels
{
    public sealed class CryptoPaymentRequestDto(
        string id,
        XDaiValue amount,
        string cryptoDisplayName,
        string cryptoSymbol,
        double exchangeRate,
        TimeSpan recalculateAfter,
        string status,
        string wallet)
    {
        public string Id { get; } = id;
        public XDaiValue Amount { get; } = amount;
        public string CryptoDisplayName { get; } = cryptoDisplayName;
        public string CryptoSymbol { get; } = cryptoSymbol;
        public double ExchangeRate { get; } = exchangeRate;
        public TimeSpan RecalculateAfter { get; } = recalculateAfter;
        public string Status { get; } = status;
        public string Wallet { get; } = wallet;
    }
}
