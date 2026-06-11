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

using Etherna.SwarmSdk.Models;
using System;
using System.Globalization;
using System.Text;

namespace Etherna.Credit.Extensions
{
    public static class XDaiValueExtensions
    {
        /// <summary>
        /// Format a XDaiBalance to a financial string
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="allowedDecimals">Max number of allowed decimals. Skip if null</param>
        /// <param name="printedDecimals">Min number of decimals to show</param>
        /// <param name="roundMode">Mode of rounding to use</param>
        /// <param name="prefixSymbol">Optional prefix string</param>
        /// <param name="suffixSymbol">Optional suffic string</param>
        /// <param name="usePlusSign">Use '+' as prefix if positive</param>
        /// <returns>The converted string</returns>
        public static string ToFinancialString(
            this XDaiValue value,
            int? allowedDecimals = 2,
            int printedDecimals = 2,
            MidpointRounding roundMode = MidpointRounding.AwayFromZero,
            string? prefixSymbol = null,
            string? suffixSymbol = " xDAI",
            bool usePlusSign = false)
        {
            if (allowedDecimals < 0)
                throw new ArgumentOutOfRangeException(nameof(allowedDecimals), "Value can't be negative");
            if (printedDecimals < 0)
                throw new ArgumentOutOfRangeException(nameof(printedDecimals), "Value can't be negative");
            if (allowedDecimals < printedDecimals)
                throw new ArgumentOutOfRangeException(
                    nameof(allowedDecimals),
                    "Allowed decimals must be equal or greater than printed decimals");

            // Round decimals.
            var decimalValue = value.ToDecimal();
            if (allowedDecimals.HasValue)
                decimalValue = Math.Round(decimalValue, allowedDecimals.Value, roundMode);

            // Print missing decimals.
            var valueStr = decimalValue.ToString(CultureInfo.InvariantCulture);
            if (printedDecimals > 0)
            {
                var valueStrBuilder = new StringBuilder(valueStr);

                var decimals = 0;
                if (valueStr.Contains('.', StringComparison.InvariantCulture))
                    decimals = valueStr.Length - valueStr.IndexOf('.', StringComparison.InvariantCulture) - 1;
                else
                    valueStrBuilder.Append('.');

                for (; decimals < printedDecimals; decimals++)
                    valueStrBuilder.Append('0');

                valueStr = valueStrBuilder.ToString();
            }

            //format as: [sign][previx]{value}[suffix]
            var strBuilder = new StringBuilder();

            if (usePlusSign && decimalValue >= 0)
                strBuilder.Append('+');
            
            strBuilder.Append(valueStr);
            
            strBuilder.Insert(
                strBuilder[0] == '+' || strBuilder[0] == '-' ? 1 : 0,
                prefixSymbol);

            strBuilder.Append(suffixSymbol);

            return strBuilder.ToString();
        }
    }
}
