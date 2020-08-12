using Etherna.EthernaCredit.Domain.Models;
using Etherna.MongODM;
using Etherna.MongODM.Serialization;
using Etherna.MongODM.Serialization.Serializers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace Etherna.EthernaCredit.Persistence.ModelMaps
{
    class UserMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.DocumentSchemaRegister.RegisterModelSchema<User>("0.2.0",
                cm =>
                {
                    cm.AutoMap();

                    // Set members with custom serializers.
                });
        }

        /// <summary>
        /// The full entity serializer without relations
        /// </summary>
        public static ReferenceSerializer<User, string> InformationSerializer(
            IDbContext dbContext,
            bool useCascadeDelete = false) =>
            new ReferenceSerializer<User, string>(dbContext, useCascadeDelete)
                .RegisterType<ModelBase>()
                .RegisterType<EntityModelBase>(cm => { })
                .RegisterType<EntityModelBase<string>>(cm =>
                {
                    cm.MapIdMember(m => m.Id);
                    cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                })
                .RegisterType<User>(cm =>
                {
                    cm.MapMember(u => u.Address);
                })
                .RegisterProxyType<User>();
    }
}
