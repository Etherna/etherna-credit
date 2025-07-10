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

using Etherna.CreditSystem.Domain.Models;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Bson.Serialization.Options;
using Etherna.MongoDB.Bson.Serialization.Serializers;

namespace Etherna.CreditSystem.Persistence.Serializers
{
    public class XDaiBalanceSerializer : StructSerializerBase<XDaiBalance>, IRepresentationConfigurable<XDaiBalanceSerializer>, IRepresentationConverterConfigurable<XDaiBalanceSerializer>
    {
        // Fields.
        private readonly Decimal128Serializer balanceSerializer;

        // Constructors.
        /// <summary>
        /// Initializes a new instance of the <see cref="XDaiBalanceSerializer"/> class.
        /// </summary>
        public XDaiBalanceSerializer()
            : this(BsonType.String)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XDaiBalanceSerializer"/> class.
        /// </summary>
        /// <param name="representation">The representation.</param>
        public XDaiBalanceSerializer(BsonType representation)
            : this(representation, new RepresentationConverter(false, false))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XDaiBalanceSerializer"/> class.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <param name="converter">The converter.</param>
        public XDaiBalanceSerializer(BsonType representation, RepresentationConverter converter)
        {
            balanceSerializer = new Decimal128Serializer(representation, converter);
        }
        
        // Properties.
        public RepresentationConverter Converter => balanceSerializer.Converter;
        public BsonType Representation => balanceSerializer.Representation;
        
        // Methods.
        public override XDaiBalance Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var value = balanceSerializer.Deserialize(context, args);
            return new XDaiBalance(Decimal128.ToDecimal(value));
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, XDaiBalance value) =>
            balanceSerializer.Serialize(context, args, new Decimal128(value.ToDecimal()));
        
        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified item serializer.
        /// </summary>
        /// <param name="converter">The converter.</param>
        /// <returns>The reconfigured serializer.</returns>
        public XDaiBalanceSerializer WithConverter(RepresentationConverter converter) =>
            converter == Converter ?
                this :
                new XDaiBalanceSerializer(Representation, converter);

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified representation.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <returns>The reconfigured serializer.</returns>
        public XDaiBalanceSerializer WithRepresentation(BsonType representation) =>
            representation == Representation ?
                this :
                new XDaiBalanceSerializer(representation, Converter);
        
        // Explicit interface implementations.
        IBsonSerializer IRepresentationConverterConfigurable.WithConverter(RepresentationConverter converter) =>
            WithConverter(converter);

        IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation) =>
            WithRepresentation(representation);
    }
}