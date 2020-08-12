﻿using Etherna.EthernaCredit.Domain.Models;
using Etherna.MongODM;
using Etherna.MongODM.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace Etherna.EthernaCredit.Persistence.ModelMaps
{
    class ModelBaseMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            // register class maps.
            dbContext.DocumentSchemaRegister.RegisterModelSchema<ModelBase>("0.2.0");

            dbContext.DocumentSchemaRegister.RegisterModelSchema<EntityModelBase<string>>("0.2.0",
                cm =>
                {
                    cm.AutoMap();

                    // Set Id representation.
                    cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId))
                                  .SetIdGenerator(new StringObjectIdGenerator());
                });
        }
    }
}
