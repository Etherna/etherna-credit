using Etherna.MongoDB.Bson;
using System;
using System.Globalization;
using System.Text;

namespace Etherna.CreditSystem.Extensions
{
    public static class DecimalExtensions
    {
        /// <summary>
        /// Format a decimal to a financial string
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
            this Decimal128 value,
            int? allowedDecimals = 2,
            int printedDecimals = 2,
            MidpointRounding roundMode = MidpointRounding.AwayFromZero,
            string? prefixSymbol = null,
            string? suffixSymbol = null,
            bool usePlusSign = false) =>
            ToFinancialString(Decimal128.ToDecimal(value), allowedDecimals, printedDecimals, roundMode, prefixSymbol, suffixSymbol, usePlusSign);

        /// <summary>
        /// Format a decimal to a financial string
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
            this decimal value,
            int? allowedDecimals = 2,
            int printedDecimals = 2,
            MidpointRounding roundMode = MidpointRounding.AwayFromZero,
            string? prefixSymbol = null,
            string? suffixSymbol = null,
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
            if (allowedDecimals.HasValue)
                value = Math.Round(value, allowedDecimals.Value, roundMode);

            // Print missing decimals.
            var valueStr = value.ToString(CultureInfo.InvariantCulture);
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

            if (usePlusSign && value >= 0)
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
