// Copyright 2021-present Etherna Sa
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.CreditSystem.Domain.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.ModelBinders
{
    public class XDaiBalanceModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext, nameof(bindingContext));

            var modelName = bindingContext.ModelName;
            
            // Try to fetch the value of the argument by name.
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            // Check if the argument value is null or empty.
            if (string.IsNullOrEmpty(value))
                return Task.CompletedTask;

            if (!decimal.TryParse(value,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var balance))
            {
                bindingContext.ModelState.TryAddModelError(
                    modelName, "Balance must be a number.");

                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(new XDaiBalance(balance));
            return Task.CompletedTask;
        }
    }
}