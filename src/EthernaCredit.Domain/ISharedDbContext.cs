using Etherna.CreditSystem.Domain.Models.UserAgg;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Repositories;

namespace Etherna.CreditSystem.Domain
{
    /// <summary>
    /// Shared DbContext between Etherna services. It's managed by SSO Server, use in read-only mode.
    /// </summary>
    public interface ISharedDbContext : IDbContext
    {
        ICollectionRepository<UserSharedInfo, string> UsersInfo { get; }
    }
}
