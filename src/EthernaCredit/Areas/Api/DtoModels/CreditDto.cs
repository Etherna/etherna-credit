using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Api.DtoModels
{
    public class CreditDto
    {
        public CreditDto(double balance, bool isUnlimited)
        {
            Balance = balance;
            IsUnlimited = isUnlimited;
        }

        public double Balance { get; }
        public bool IsUnlimited { get; }
    }
}
