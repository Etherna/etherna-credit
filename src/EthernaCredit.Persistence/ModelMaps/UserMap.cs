using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.UserAgg;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;
using Etherna.MongODM.Core.Serialization.Serializers;

namespace Etherna.CreditSystem.Persistence.ModelMaps
{
    class UserMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.SchemaRegistry.AddModelMapsSchema<User>("0ff83163-b49f-4182-895d-bed59e73a976");
            dbContext.SchemaRegistry.AddModelMapsSchema<UserBalance>("873c5ee4-122b-4021-8dc9-524b9f50b73b",
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(b => b.Credit, new Decimal128Serializer(BsonType.Decimal128));
                    mm.SetMemberSerializer(b => b.User, ReferenceSerializer(dbContext));
                });
        }

        /// <summary>
        /// The document reference with only Id.
        /// </summary>
        public static ReferenceSerializer<User, string> ReferenceSerializer(
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
                config.AddModelMapsSchema<User>("b309c982-f30f-46ad-b076-c6030c8dbcd8", mm => { });
            });
    }
}
