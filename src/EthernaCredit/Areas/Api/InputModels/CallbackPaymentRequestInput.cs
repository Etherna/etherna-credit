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

using System.Diagnostics.CodeAnalysis;

namespace Etherna.Credit.Areas.Api.InputModels
{
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
    public sealed class CallbackPaymentRequestInput
    {
        /// <summary>
        /// Invoice or Order ID in the external system
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("external_id")]
        public string ExternalId { get; set; } = null!;

        /// <summary>
        /// Cryptocurrency (provided during payment request creation)
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("crypto")]
        public string Crypto { get; set; } = null!;

        /// <summary>
        /// SHKeeper invoice wallet address that receives payments
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("addr")]
        public string Address { get; set; } = null!;

        /// <summary>
        /// Fiat currency (provided during payment request creation)
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("fiat")]
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<CallbackPaymentRequestFiatInput>))]
        public CallbackPaymentRequestFiatInput FiatInput { get; set; } = default!;

        /// <summary>
        /// SHKeeper invoice amount in fiat currency
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("balance_fiat")]
        public string BalanceFiat { get; set; } = null!;

        /// <summary>
        /// SHKeeper invoice amount in cryptocurrency
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("balance_crypto")]
        public string BalanceCrypto { get; set; } = null!;

        /// <summary>
        /// `true` if the payment request is fully paid, `false` if only a partial or no payment is received
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("paid")]
        public bool Paid { get; set; } = false!;

        /// <summary>
        /// SHKeeper invoice status
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<CallbackPaymentRequestStatusInput>))]
        public CallbackPaymentRequestStatusInput StatusInput { get; set; } = default!;

        /// <summary>
        /// In case of SHKeeper invoice status `OVERPAID`, the overpaid amount will be shown here
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("overpaid_fiat")]
        public string OverpaidFiat { get; set; } = null!;

        /// <summary>
        /// SHKeeper fee percentage added to the invoice amount
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("fee_percent")]
        public string FeePercent { get; set; } = null!;

        /// <summary>
        /// SHKeeper fixed fee in fiat added to the invoice amount
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("fee_fixed")]
        public string FeeFixed { get; set; } = null!;

        /// <summary>
        /// SHKeeper fee calculation policy. `PERCENT_OR_MINIMAL_FIXED_FEE` - use fixed fee value if percent from amount less than fixed
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("fee_policy")]
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter<CallbackPaymentRequestFeePolicyInput>))]
        public CallbackPaymentRequestFeePolicyInput FeePolicyInput { get; set; } = default!;

        /// <summary>
        /// SHKeeper invoice transactions
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("transactions")]
        public System.Collections.Generic.ICollection<CallbackTransactionInput> Transactions { get; set; } = [];
    }
}