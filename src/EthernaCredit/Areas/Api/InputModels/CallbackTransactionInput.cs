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

namespace Etherna.Credit.Areas.Api.InputModels
{
    public sealed class CallbackTransactionInput
    {
        /// <summary>
        /// Transaction ID
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("txid")]
        public string TxId { get; set; } = null!;

        /// <summary>
        /// Transaction date
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("date")]
        public string Date { get; set; } = null!;

        /// <summary>
        /// Transaction amount in cryptocurrency
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("amount_crypto")]
        public string AmountCrypto { get; set; } = null!;

        /// <summary>
        /// Transaction amount in fiat currency
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("amount_fiat")]
        public string AmountFiat { get; set; } = null!;

        /// <summary>
        /// Transaction amount in fiat currency without SHKeeper fee. Can be negative in case of `FIXED_FEE` or `PERCENT_OR_MINIMAL_FIXED_FEE` policy if transaction `amount_fiat` less than `fee_fixed`
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("amount_fiat_without_fee")]
        public string AmountFiatWithoutFee { get; set; } = null!;

        /// <summary>
        /// SHKeeper fee in fiat for transaction calculated according chosen policy for related cryptocurrency
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("fee_fiat")]
        public string FeeFiat { get; set; } = null!;

        /// <summary>
        /// `true` if this transaction was the trigger for the callback
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("trigger")]
        public bool Trigger { get; set; } = false;

        /// <summary>
        /// Transaction cryptocurrency
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("crypto")]
        public string Crypto { get; set; } = null!;
    }
}