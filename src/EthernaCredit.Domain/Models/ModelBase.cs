﻿using Etherna.MongODM.Core.Domain.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Etherna.CreditSystem.Domain.Models
{
    public abstract class ModelBase : IModel
    {
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Setter needed for deserialization scope")]
        public virtual IDictionary<string, object>? ExtraElements { get; protected set; }
    }
}
