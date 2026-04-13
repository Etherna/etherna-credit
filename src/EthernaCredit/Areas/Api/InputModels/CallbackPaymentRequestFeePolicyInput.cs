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
    public enum CallbackPaymentRequestFeePolicyInput
    {
        [System.Text.Json.Serialization.JsonStringEnumMemberName("NO_FEE")]
        NoFee = 0,

        [System.Text.Json.Serialization.JsonStringEnumMemberName("PERCENT_FEE")]
        PercentFee = 1,

        [System.Text.Json.Serialization.JsonStringEnumMemberName("FIXED_FEE")]
        FixedFee = 2,

        [System.Text.Json.Serialization.JsonStringEnumMemberName("PERCENT_OR_MINIMAL_FIXED_FEE")]
        PercentOrMinimalFixedFee = 3
    }
}