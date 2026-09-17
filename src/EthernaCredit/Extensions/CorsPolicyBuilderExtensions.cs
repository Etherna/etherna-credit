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

using Etherna.ACR.Exceptions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Etherna.Credit.Extensions
{
    public static class CorsPolicyBuilderExtensions
    {
        // Consts.
        private const string AllowedOriginsConfigKey = "Cors:AllowedOrigins";

        // Methods.
        /// <summary>
        /// Configures the cross-origin policy of the host: any origin in Development, only the origins
        /// listed by <c>Cors:AllowedOrigins</c> in every other environment, always with credentials.
        /// </summary>
        /// <exception cref="ServiceConfigurationException">
        /// Outside Development, no origin is configured or an entry is not a bare web origin.
        /// </exception>
        public static CorsPolicyBuilder ConfigureCreditPolicy(
            this CorsPolicyBuilder builder,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(environment);

            if (environment.IsDevelopment())
            {
                builder.SetIsOriginAllowed(_ => true);
            }
            else
            {
                // An empty list, or an entry a browser's Origin header can never equal (a trailing slash,
                // a path, a missing scheme), would silently close the api to the client: refuse to start instead.
                var allowedOrigins = configuration.GetSection(AllowedOriginsConfigKey).Get<string[]>() ?? [];
                if (allowedOrigins.Length == 0)
                    throw new ServiceConfigurationException(
                        $"{AllowedOriginsConfigKey} must list at least one origin outside the Development environment");

                var malformedOrigin = allowedOrigins.FirstOrDefault(origin => !IsWebOrigin(origin));
                if (malformedOrigin is not null)
                    throw new ServiceConfigurationException(
                        $"{AllowedOriginsConfigKey} entry \"{malformedOrigin}\" is not a web origin: expected scheme://host[:port], http or https, with no path");

                builder.WithOrigins(allowedOrigins);
            }

            return builder.AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
        }

        // Helpers.
        private static bool IsWebOrigin(string origin) =>
            Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && uri.UserInfo.Length == 0
            && string.Equals(uri.GetLeftPart(UriPartial.Authority), origin, StringComparison.OrdinalIgnoreCase);
    }
}
