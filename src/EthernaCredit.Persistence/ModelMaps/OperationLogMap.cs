using Etherna.EthernaCredit.Domain.Models;
using Etherna.EthernaCredit.Domain.Models.OperationLogs;
using Etherna.MongODM;
using Etherna.MongODM.Extensions;
using Etherna.MongODM.Serialization;

namespace Etherna.EthernaCredit.Persistence.ModelMaps
{
    class OperationLogMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.DocumentSchemaRegister.RegisterModelSchema<OperationLogBase>("0.2.0",
                cm =>
                {
                    cm.AutoMap();

                    // Set members with custom serializers.
                    cm.SetMemberSerializer(l => l.User, UserMap.InformationSerializer(dbContext));
                });

            dbContext.DocumentSchemaRegister.RegisterModelSchema<DepositOperationLog>("0.2.0");

            dbContext.DocumentSchemaRegister.RegisterModelSchema<UpdateOperationLog>("0.2.0");

            dbContext.DocumentSchemaRegister.RegisterModelSchema<WithdrawOperationLog>("0.2.0");
        }
    }
}
