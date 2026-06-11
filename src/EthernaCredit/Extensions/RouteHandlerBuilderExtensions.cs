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

using Etherna.Credit.Configs.OpenApi;
using Microsoft.AspNetCore.Builder;

namespace Etherna.Credit.Extensions
{
    public static class RouteHandlerBuilderExtensions
    {
        public static RouteHandlerBuilder IsDeprecated(this RouteHandlerBuilder builder, string? message = null) =>
            builder.WithMetadata(new DeprecatedEndpointMetadata(message));
        
        /// <summary>
        /// Required because of https://github.com/dotnet/aspnetcore/issues/43330
        /// </summary>
        public static RouteHandlerBuilder NotProduces200(this RouteHandlerBuilder builder) =>
            builder.WithMetadata(new RemoveResponse200EndpointMetadata());
    }
}