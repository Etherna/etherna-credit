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

using Etherna.MongoDB.Bson;
using System;
using System.Globalization;

namespace Etherna.CreditSystem.Domain.Models;

public struct XDaiBalance : IEquatable<XDaiBalance>
{
    // Consts.
    public const int DecimalPrecision = 18;
    
    // Fields.
    private readonly decimal _balance;
    
    // Constructors.
    public XDaiBalance(decimal balance)
    {
        _balance = decimal.Round(balance, DecimalPrecision)
                   / 1.000000000000000000000000000000000m; //remove final zeros
    }
    
    // Methods.
    public int CompareTo(XDaiBalance other) => _balance.CompareTo(other._balance);
    public override bool Equals(object? obj) =>
        obj is XDaiBalance xDaiObj &&
        Equals(xDaiObj);
    public bool Equals(XDaiBalance other) => _balance == other._balance;
    public override int GetHashCode() => _balance.GetHashCode();
    public decimal ToDecimal() => _balance;
    public override string ToString() => _balance.ToString(CultureInfo.InvariantCulture);
    
    // Static methods.
    public static XDaiBalance Add(XDaiBalance left, XDaiBalance right) => left + right;
    public static XDaiBalance Decrement(XDaiBalance balance)=> --balance;
    public static XDaiBalance Divide(XDaiBalance left, XDaiBalance right) => left / right;
    public static XDaiBalance FromDecimal(decimal value) => new(value);
    public static XDaiBalance FromDouble(double value) => new((decimal)value);
    public static XDaiBalance FromInt32(int value) => new(value);
    public static XDaiBalance Increment(XDaiBalance balance) => ++balance;
    public static XDaiBalance Multiply(XDaiBalance left, XDaiBalance right) => left * right;
    public static XDaiBalance Negate(XDaiBalance balance) => -balance;
    public static XDaiBalance Subtract(XDaiBalance left, XDaiBalance right) => left - right;

    // Operator methods.
    public static XDaiBalance operator +(XDaiBalance left, XDaiBalance right) => new(left._balance + right._balance);
    public static XDaiBalance operator -(XDaiBalance left, XDaiBalance right) => new(left._balance - right._balance);
    public static XDaiBalance operator *(XDaiBalance left, XDaiBalance right) => new(left._balance * right._balance);
    public static XDaiBalance operator /(XDaiBalance left, XDaiBalance right) => new(left._balance / right._balance);
    public static bool operator ==(XDaiBalance left, XDaiBalance right) => left.Equals(right);
    public static bool operator !=(XDaiBalance left, XDaiBalance right) => !(left == right);
    public static bool operator >(XDaiBalance left, XDaiBalance right) => left._balance > right._balance;
    public static bool operator <(XDaiBalance left, XDaiBalance right) => left._balance < right._balance;
    public static bool operator >=(XDaiBalance left, XDaiBalance right) => left._balance >= right._balance;
    public static bool operator <=(XDaiBalance left, XDaiBalance right) => left._balance <= right._balance;
    public static XDaiBalance operator -(XDaiBalance value) => new(-value._balance);
    public static XDaiBalance operator ++(XDaiBalance value) => new(value._balance + 1);
    public static XDaiBalance operator --(XDaiBalance value) => new(value._balance - 1);
    
    // Implicit conversion operator methods.
    public static implicit operator XDaiBalance(decimal value) => new(value);
    public static implicit operator XDaiBalance(double value) => new((decimal)value);
    public static implicit operator XDaiBalance(int value) => new(value);
    
    public static explicit operator decimal(XDaiBalance value) => value.ToDecimal();
}