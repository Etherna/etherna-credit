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

using Etherna.Authentication;
using Etherna.Credit.Configs;
using Etherna.Credit.Domain;
using Etherna.Credit.Domain.Models;
using Etherna.Credit.Domain.Models.UserAgg;
using Etherna.Credit.Extensions;
using Etherna.Credit.Services.Domain;
using Etherna.Scrinium.Core.Repositories;
using Etherna.SwarmSdk.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.Credit.Areas.Api
{
    public class CreditApiMapperTest
    {
        // Consts.
        private const string UserAddress = "0x1111111111111111111111111111111111111111";

        // Tests.
        [Theory]
        [InlineData("page=-1&take=50", "page")]
        [InlineData("page=0&take=0", "take")]
        [InlineData("page=0&take=1001", "take")]
        public async Task MapCreditApi_UserLogsWithOutOfRangeValue_AnswersValidationProblem(
            string query,
            string invalidParameter)
        {
            // Arrange.
            await using var app = await StartApplicationAsync();
            using var client = app.GetTestClient();

            // Action.
            using var response = await client.GetAsync(new Uri($"/api/v0.3/user/logs?{query}", UriKind.Relative));
            var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();

            // Assert.
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal([invalidParameter], problem.Errors.Keys);
        }

        [Theory]
        [InlineData("")]
        [InlineData("?page=0&take=1")]
        [InlineData("?page=3&take=1000")]
        public async Task MapCreditApi_UserLogsWithValuesInRange_AnswersOk(string query)
        {
            // Arrange.
            await using var app = await StartApplicationAsync();
            using var client = app.GetTestClient();

            // Action.
            using var response = await client.GetAsync(new Uri($"/api/v0.3/user/logs{query}", UriKind.Relative));

            // Assert.
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Helpers.
        private static async Task<WebApplication> StartApplicationAsync()
        {
            // Mock the handler's dependencies, with a user owning no logs.
            var userMock = new Mock<User>();
            userMock.Setup(u => u.Id).Returns("6502fe5c1c5a4a4a9c6f0b1b");

            var ethernaOidcClientMock = new Mock<IEthernaOpenIdConnectClient>();
            ethernaOidcClientMock.Setup(c => c.GetEtherAddressAsync()).ReturnsAsync(UserAddress);

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(s => s.FindUserAsync(It.IsAny<EthAddress>()))
                .ReturnsAsync((userMock.Object, new Mock<UserSharedInfo>().Object));

            var dbContextMock = new Mock<ICreditDbContext>();
            dbContextMock.Setup(c => c.OperationLogs.QueryPaginatedElementsAsync(
                    It.IsAny<Func<IQueryable<OperationLogBase>, IQueryable<OperationLogBase>>>(),
                    It.IsAny<Expression<Func<OperationLogBase, DateTime>>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaginatedEnumerable<OperationLogBase>([], 0, 0, 50));

            // Build the application with the api as the host composes it, open to anonymous requests.
            var builder = WebApplication.CreateSlimBuilder();
            builder.Logging.ClearProviders();
            builder.WebHost.UseTestServer();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(CommonConsts.ServiceInteractApiScopePolicy, policy => policy.RequireAssertion(_ => true));
                options.AddPolicy(CommonConsts.UserInteractApiScopePolicy, policy => policy.RequireAssertion(_ => true));
            });
            builder.Services.AddCreditApi();
            builder.Services.AddScoped(_ => dbContextMock.Object);
            builder.Services.AddScoped(_ => ethernaOidcClientMock.Object);
            builder.Services.AddScoped(_ => userServiceMock.Object);

            var app = builder.Build();
            app.UseAuthorization();
            app.MapCreditApi();

            await app.StartAsync();
            return app;
        }
    }
}
