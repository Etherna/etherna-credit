using Etherna.CreditSystem.Domain.Models.UserAgg;
using Etherna.MongODM.Core.Repositories;

namespace Etherna.CreditSystem.Domain
{
    /// <summary>
    /// Don't access directly to this. 
    /// This context exposes models unmanaged from domain space.
    /// Interact only with IUserService.
    /// </summary>
    public interface ICreditDbContextInternal : ICreditDbContext
    {
        ICollectionRepository<UserBalance, string> UserBalances { get; }
    }
}
