using Etherna.MongoDB.Bson;
using System;

namespace Etherna.CreditSystem.Domain.Models.UserAgg
{
    /// <summary>
    /// This class is unmanaged from domain space.
    /// It needs atomic direct operations on db, interact only with IUserService.
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
