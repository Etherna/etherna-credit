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
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Etherna.Credit.Areas.Api
{
    internal interface ICreditApiHandler
    {
        Task<IResult> CreateCryptoInvoiceAsync(XDaiValue amount, string cryptoSymbol);
        Task<IResult> GetAvailablePaymentCryptosAsync();
        Task<IResult> GetCryptoInvoiceAsync(string id);
        Task<IResult> GetCurrentUserAddressAsync();
        Task<IResult> GetCurrentUserCreditAsync();
        Task<IResult> GetCurrentUserLogsAsync(int page, int take);
        Task<IResult> GetServiceOpLogsWithUserAsync(EthAddress address, DateTime? fromDate, DateTime? toDate);
        Task<IResult> GetUserCreditAsync(EthAddress address);
        Task<IResult> RegisterBalanceUpdateAsync(EthAddress address, XDaiValue amount, bool isApplied, string reason);
    }
}