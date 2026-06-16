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

using Etherna.Credit.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Etherna.Credit.Configs.ModelBinding
{
    /// <summary>
    /// MVC/Razor Pages model binder that binds an XDaiValue from its decimal xDAI representation,
    /// consistently with JSON/BSON serialization and the Minimal API XDaiValueDecimalParameter. The
    /// SwarmSdk default (its TypeConverter) parses strings as wei integers instead, which is
    /// inconsistent with how the rest of the application represents amounts.
    /// </summary>
    public sealed class XDaiValueModelBinder : IModelBinder
    {
        // Methods.
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            if (XDaiValueExtensions.TryParse(valueProviderResult.FirstValue, CultureInfo.InvariantCulture, out var result))
                bindingContext.Result = ModelBindingResult.Success(result);
            else
                bindingContext.ModelState.TryAddModelError(
                    modelName,
                    bindingContext.ModelMetadata.ModelBindingMessageProvider.ValueIsInvalidAccessor(
                        valueProviderResult.ToString()));

            return Task.CompletedTask;
        }
    }
}
