using Etherna.MongoDB.Bson;

namespace Etherna.CreditSystem.Domain.Models.UserAgg
{
    /// <summary>
    /// This class is unmanaged from domain space.
    /// It needs atomic direct operations on db, and Id is an external key for User.Id.
    /// Interact only with IUserService.
    /// </summary>
    public class UserBalance : EntityModelBase<string>
    {
        // Constructors.
        public UserBalance(User user)
        {
            User = user;
        }
        protected UserBalance() { }

        // Properties.
        public virtual Decimal128 Credit { get; protected set; }
        public virtual User User { get; protected set; } = default!;
    }
}
