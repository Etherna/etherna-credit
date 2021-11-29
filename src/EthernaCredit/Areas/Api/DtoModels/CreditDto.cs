namespace Etherna.CreditSystem.Areas.Api.DtoModels
{
    public class CreditDto
    {
        public CreditDto(decimal balance, bool isUnlimited)
        {
            Balance = balance;
            IsUnlimited = isUnlimited;
        }

        public decimal Balance { get; }
        public bool IsUnlimited { get; }
    }
}
