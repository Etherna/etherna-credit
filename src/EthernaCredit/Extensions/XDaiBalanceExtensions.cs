//   Copyright 2021-present Etherna Sa
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.CreditSystem.Domain.Models;
using System;
using System.Globalization;
using System.Text;

namespace Etherna.CreditSystem.Extensions
{
    public static class XDaiBalanceExtensions
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
            this XDaiBalance value,
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
