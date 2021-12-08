using Etherna.CreditSystem.Domain.Models;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization.IdGenerators;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.CreditSystem.Persistence.ModelMaps
{
    class ModelBaseMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            // register class maps.
            dbContext.SchemaRegister.AddModelMapsSchema<ModelBase>("92a5e4a2-6b0c-46b7-aac7-70380deda7b4");
            dbContext.SchemaRegister.AddModelMapsSchema<EntityModelBase>("155af6aa-92d1-4d8b-b000-5b5464f84dc7");
            dbContext.SchemaRegister.AddModelMapsSchema<EntityModelBase<string>>("a0fddc7f-b64d-43b0-a40d-389b3fb21b67",
                mm =>
                {
                    mm.AutoMap();

                    // Set Id representation.
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId))
                                  .SetIdGenerator(new StringObjectIdGenerator());
                });
        }
    }
}
