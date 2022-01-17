using Etherna.CreditSystem.Domain.Models;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization.IdGenerators;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.CreditSystem.Persistence.ModelMaps.Shared
{
    class ModelBaseMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            // register class maps.
            dbContext.SchemaRegistry.AddModelMapsSchema<ModelBase>("d517f32d-cc45-4d21-8a99-27dca658bde5");
            dbContext.SchemaRegistry.AddModelMapsSchema<EntityModelBase>("4c17bb54-af84-4a21-83ae-cb1050b721f5");
            dbContext.SchemaRegistry.AddModelMapsSchema<EntityModelBase<string>>("e5e834e0-30cc-42a8-a1a2-9d5c79d35485",
                modelMap =>
                {
                    modelMap.AutoMap();

                    // Set Id representation.
                    modelMap.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId))
                                        .SetIdGenerator(new StringObjectIdGenerator());
                });
        }
    }
}
