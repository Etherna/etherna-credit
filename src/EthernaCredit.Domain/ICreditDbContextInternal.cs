using Etherna.CreditSystem.Domain.Models.UserAgg;
using Etherna.MongODM.Core.Repositories;

namespace Etherna.CreditSystem.Domain
{
    public interface ICreditDbContextInternal : ICreditDbContext
    {
        ICollectionRepository<UserBalance, string> UserBalances { get; }
    }
}
