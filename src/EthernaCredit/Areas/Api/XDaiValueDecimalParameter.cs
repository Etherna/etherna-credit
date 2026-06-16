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

using Etherna.Credit.Extensions;
using Etherna.SwarmSdk.Models;
using System;

namespace Etherna.Credit.Areas.Api
{
    /// <summary>
    /// Binds an <see cref="XDaiValue"/> from a Minimal API route/query parameter using its decimal
    /// xDAI representation. <see cref="XDaiValue"/>'s own <see cref="IParsable{TSelf}"/> implementation
    /// parses the wei integer string instead, which is inconsistent with how the rest of the application
    /// represents amounts (JSON and BSON serialization both use the decimal xDAI value). Declare endpoint
    /// parameters as <see cref="XDaiValueDecimalParameter"/> and pass them straight through: the implicit
    /// conversion yields the underlying <see cref="XDaiValue"/>, so no manual conversion is needed.
    /// </summary>
    public readonly record struct XDaiValueDecimalParameter(XDaiValue Value) :
        IParsable<XDaiValueDecimalParameter>
    {
        // Methods.
        public static XDaiValueDecimalParameter Parse(string s, IFormatProvider? provider) =>
            TryParse(s, provider, out var result)
                ? result
                : throw new FormatException($"'{s}' is not a valid decimal xDAI value.");

        public XDaiValue ToXDaiValue() => Value;

        public static bool TryParse(string? s, IFormatProvider? provider, out XDaiValueDecimalParameter result)
        {
            if (XDaiValueExtensions.TryParse(s, provider, out var value))
            {
                result = new XDaiValueDecimalParameter(value);
                return true;
            }

            result = default;
            return false;
        }

        // Operators.
        public static implicit operator XDaiValue(XDaiValueDecimalParameter parameter) => parameter.Value;
    }
}
