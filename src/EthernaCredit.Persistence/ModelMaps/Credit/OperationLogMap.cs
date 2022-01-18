using Etherna.CreditSystem.Domain.Models;
using Etherna.CreditSystem.Domain.Models.OperationLogs;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.CreditSystem.Persistence.ModelMaps.Credit
{
    class OperationLogMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.SchemaRegistry.AddModelMapsSchema<OperationLogBase>("780ae0cc-070a-4099-a4c0-0494304e4093",
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(l => l.Amount, new Decimal128Serializer(BsonType.Decimal128));
                    mm.SetMemberSerializer(l => l.User, UserMap.ReferenceSerializer(dbContext));
                });
            dbContext.SchemaRegistry.AddModelMapsSchema<DepositOperationLog>("7fc7abe8-9a55-40a2-90ce-f3cba34bc005");
            dbContext.SchemaRegistry.AddModelMapsSchema<UpdateOperationLog>("74e021d4-6d86-4deb-b952-0c328839cfe2");
            dbContext.SchemaRegistry.AddModelMapsSchema<WithdrawOperationLog>("b0ffe059-c985-4f3d-8677-238ab9551ec3");
        }
    }
}
