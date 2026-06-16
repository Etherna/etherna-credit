// Copyright 2021-present Etherna SA
// This file is part of Etherna Credit.
//
// Etherna Credit is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// Etherna Credit is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License along with Etherna Credit.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.SwarmSdk.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace Etherna.Credit.Configs.ModelBinding
{
    /// <summary>
    /// Provides the <see cref="XDaiValueModelBinder"/> for <see cref="XDaiValue"/> model types, so that
    /// MVC/Razor Pages bind xDAI amounts from their decimal representation instead of the SwarmSdk
    /// TypeConverter default (wei integers).
    /// </summary>
    public sealed class XDaiValueModelBinderProvider : IModelBinderProvider
    {
        // Methods.
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Metadata.ModelType == typeof(XDaiValue)
                ? new XDaiValueModelBinder()
                : null;
        }
    }
}
