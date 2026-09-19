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

using Etherna.Credit.Areas.Api;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Etherna.Credit.Extensions
{
    public static class ServiceCollectionExtensions
    {
        // Methods.
        /// <summary>
        /// Registers the services the routes mapped by <see cref="CreditApiMapper.MapCreditApi"/> run on:
        /// the API handler, and the validation of the route handlers' parameters.
        /// </summary>
        public static IServiceCollection AddCreditApi(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddScoped<ICreditApiHandler, CreditApiHandler>();

            // Minimal APIs run the validation attributes of their parameters only with these services:
            // without them the attributes do nothing but decorate the OpenAPI document.
            services.AddValidation();

            return services;
        }
    }
}
