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
        public UserBalance(string id)
        {
            Id = id;
        }
        protected UserBalance() { }

        // Properties.
        public virtual double Credit { get; protected set; }
    }
}
