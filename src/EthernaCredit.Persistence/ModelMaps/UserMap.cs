﻿using Etherna.CreditSystem.Domain.Models;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;
using Etherna.MongODM.Core.Serialization.Serializers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace Etherna.CreditSystem.Persistence.ModelMaps
{
    class UserMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.SchemaRegister.AddModelMapsSchema<User>("0ff83163-b49f-4182-895d-bed59e73a976",
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                });
        }

        /// <summary>
        /// The full entity serializer without relations
        /// </summary>
        public static ReferenceSerializer<User, string> InformationSerializer(
            IDbContext dbContext,
            bool useCascadeDelete = false) =>
            new(dbContext, config =>
            {
                config.UseCascadeDelete = useCascadeDelete;
                config.AddModelMapsSchema<ModelBase>("485440e2-ce18-4c40-a8ca-31280fbb22ed");
                config.AddModelMapsSchema<EntityModelBase>("bac86f72-a3d9-4ccc-bb43-9af68d6d5c03", mm => { });
                config.AddModelMapsSchema<EntityModelBase<string>>("f1cc0691-f879-4036-a994-fe92b6d39cfb", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMapsSchema<User>("b309c982-f30f-46ad-b076-c6030c8dbcd8", mm =>
                {
                    mm.MapMember(u => u.Address);
                });
                config.AddModelMapsSchema<User>("117b4635-8bb3-428b-af09-81258e4a1b9b");
            });
    }
}
