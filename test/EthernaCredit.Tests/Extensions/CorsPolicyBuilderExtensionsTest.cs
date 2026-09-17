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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Etherna.Credit.Extensions
{
    public class CorsPolicyBuilderExtensionsTest
    {
        // Fields.
        private readonly CorsService corsService = new(Options.Create(new CorsOptions()), NullLoggerFactory.Instance);

        // Tests.
        [Fact]
        public void ConfigureCreditPolicy_InDevelopment_AllowsAnyOriginWithCredentials()
        {
            // Arrange.
            //no origin configured
            var policy = BuildPolicy(Environments.Development);

            // Action.
            var responseHeaders = EvaluateRequest("http://localhost:3000", policy);

            // Assert.
            Assert.Equal("http://localhost:3000", responseHeaders.AccessControlAllowOrigin);
            Assert.Equal("true", responseHeaders.AccessControlAllowCredentials);
        }

        [Theory]
        [InlineData("https://etherna.io")]
        [InlineData("https://app.etherna.io")]
        public void ConfigureCreditPolicy_OutsideDevelopment_AllowsConfiguredOriginWithCredentials(string origin)
        {
            // Arrange.
            var policy = BuildPolicy(Environments.Production, "https://etherna.io", "https://app.etherna.io");

            // Action.
            var responseHeaders = EvaluateRequest(origin, policy);

            // Assert.
            Assert.Equal(origin, responseHeaders.AccessControlAllowOrigin);
            Assert.Equal("true", responseHeaders.AccessControlAllowCredentials);
        }

        [Theory]
        [InlineData("http://localhost:3000", "http://localhost:3000")]
        [InlineData("HTTPS://App.Etherna.io", "https://app.etherna.io")]
        public void ConfigureCreditPolicy_OutsideDevelopment_AllowsOriginWithPortOrUppercase(string configuredOrigin, string origin)
        {
            // Arrange.
            var policy = BuildPolicy(Environments.Production, configuredOrigin);

            // Action.
            var responseHeaders = EvaluateRequest(origin, policy);

            // Assert.
            Assert.Equal(origin, responseHeaders.AccessControlAllowOrigin);
        }

        [Theory]
        [InlineData("https://other.example")]
        [InlineData("http://etherna.io")]
        [InlineData("https://etherna.io.other.example")]
        [InlineData("https://beta.etherna.io")]
        public void ConfigureCreditPolicy_OutsideDevelopment_RejectsUnlistedOrigin(string origin)
        {
            // Arrange.
            var policy = BuildPolicy(Environments.Production, "https://etherna.io", "https://app.etherna.io");

            // Action.
            var responseHeaders = EvaluateRequest(origin, policy);

            // Assert.
            Assert.False(responseHeaders.ContainsKey(HeaderNames.AccessControlAllowOrigin));
            Assert.False(responseHeaders.ContainsKey(HeaderNames.AccessControlAllowCredentials));
        }

        [Theory]
        [InlineData("https://etherna.io/")]
        [InlineData("https://etherna.io/watch")]
        [InlineData("https://etherna.io?ref=1")]
        [InlineData("etherna.io")]
        [InlineData("ftp://etherna.io")]
        [InlineData("https://user@etherna.io")]
        [InlineData("*")]
        [InlineData("")]
        public void ConfigureCreditPolicy_OutsideDevelopment_WithMalformedOrigin_RefusesToStart(string malformedOrigin)
        {
            // Arrange.
            var builder = new CorsPolicyBuilder();
            //a valid entry beside the malformed one: the check is per entry, not on the list
            var configuration = BuildConfiguration("https://app.etherna.io", malformedOrigin);

            // Action.
            var exception = Assert.Throws<ServiceConfigurationException>(
                () => builder.ConfigureCreditPolicy(configuration, BuildEnvironment(Environments.Production)));

            // Assert.
            Assert.Contains("Cors:AllowedOrigins", exception.Message, StringComparison.Ordinal);
            Assert.Contains($"\"{malformedOrigin}\"", exception.Message, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("Production")]
        [InlineData("Staging")]
        public void ConfigureCreditPolicy_OutsideDevelopment_WithoutOrigins_RefusesToStart(string environmentName)
        {
            // Arrange.
            var builder = new CorsPolicyBuilder();
            var configuration = new ConfigurationBuilder().Build();

            // Action.
            var exception = Assert.Throws<ServiceConfigurationException>(
                () => builder.ConfigureCreditPolicy(configuration, BuildEnvironment(environmentName)));

            // Assert.
            Assert.Contains("Cors:AllowedOrigins", exception.Message, StringComparison.Ordinal);
        }

        // Helpers.
        private static IConfiguration BuildConfiguration(params string[] allowedOrigins) =>
            //the same keys the environment variables Cors__AllowedOrigins__<n> produce
            new ConfigurationBuilder()
                .AddInMemoryCollection(allowedOrigins.Select((origin, i) =>
                    new KeyValuePair<string, string?>($"Cors:AllowedOrigins:{i}", origin)))
                .Build();

        private static IHostEnvironment BuildEnvironment(string environmentName)
        {
            var environmentMock = new Mock<IHostEnvironment>();
            environmentMock.Setup(e => e.EnvironmentName).Returns(environmentName);
            return environmentMock.Object;
        }

        private static CorsPolicy BuildPolicy(string environmentName, params string[] allowedOrigins) =>
            new CorsPolicyBuilder()
                .ConfigureCreditPolicy(BuildConfiguration(allowedOrigins), BuildEnvironment(environmentName))
                .Build();

        private IHeaderDictionary EvaluateRequest(string origin, CorsPolicy policy)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = HttpMethods.Get;
            context.Request.Headers.Origin = origin;

            var result = corsService.EvaluatePolicy(context, policy);
            corsService.ApplyResult(result, context.Response);

            return context.Response.Headers;
        }
    }
}
