using Etherna.CreditSystem.Domain.Models;
using System;

namespace Etherna.CreditSystem.Areas.Api.DtoModels
{
    public class LogDto
    {
        public LogDto(OperationLogBase log)
        {
            if (log is null)
                throw new ArgumentNullException(nameof(log));

            Ammount = log.Ammount;
            Author = log.Author;
            CreationDateTime = log.CreationDateTime;
            OperationName = log.OperationName;
        }

        public virtual double Ammount { get; }
        public virtual string Author { get; }
        public DateTime CreationDateTime { get; }
        public string OperationName { get; }
    }
}
