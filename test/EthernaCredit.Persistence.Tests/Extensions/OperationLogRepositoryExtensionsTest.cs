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

using Etherna.Credit.Domain.Models;
using Etherna.Credit.Persistence.Helpers;
using Etherna.DomainEvents;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Driver;
using Etherna.Scrinium.Core.Serialization.Serializers;
using Etherna.Scrinium.Core.Utility;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.Credit.Persistence.Extensions
{
    public class OperationLogRepositoryExtensionsTest
    {
        // Consts.
        private const string Author = "ethernaGatewayCreditClientId";
        private const string Reason = "DownloadFee";
        private const string UserId = "652d9b0bf665611b84305e57";

        // Fields.
        private readonly Mock<IMongoCollection<OperationLogBase>> collectionMock;
        private readonly CreditDbContext dbContext;
        private FilterDefinition<OperationLogBase>? sameDayLogFilter;
        private readonly User user;

        // Constructor.
        public OperationLogRepositoryExtensionsTest()
        {
            // Setup dbContext.
            var mongoDatabaseMock = new Mock<IMongoDatabase>();
            dbContext = new CreditDbContext(new Mock<IEventDispatcher>().Object, new Mock<ILogger<CreditDbContext>>().Object);
            DbContextMockHelper.InitializeDbContextMock(dbContext, mongoDatabaseMock);

            // Setup the collection, capturing the filter the log of the day is looked for with.
            collectionMock = DbContextMockHelper.SetupCollectionMock(mongoDatabaseMock, dbContext.OperationLogs);
            collectionMock.Setup(c => c.FindOneAndUpdateAsync(
                    It.IsAny<FilterDefinition<OperationLogBase>>(),
                    It.IsAny<UpdateDefinition<OperationLogBase>>(),
                    It.IsAny<FindOneAndUpdateOptions<OperationLogBase, OperationLogBase>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<FilterDefinition<OperationLogBase>, UpdateDefinition<OperationLogBase>, FindOneAndUpdateOptions<OperationLogBase, OperationLogBase>, CancellationToken>(
                    (filter, _, _, _) => sameDayLogFilter = filter)
                .ReturnsAsync((OperationLogBase)null!);

            var userMock = new Mock<User>();
            userMock.Setup(u => u.Id).Returns(UserId);
            user = userMock.Object;
        }

        // Tests.
        [Fact]
        public async Task TryIncrementSameDayUpdateLogAsync_WithAppliedUpdate_MatchesTheLogsStoredWithoutIsApplied()
        {
            // Action.
            await dbContext.OperationLogs.TryIncrementSameDayUpdateLogAsync(-1, Author, true, Reason, user);

            // Assert.
            //the model map ignores IsApplied on write when it is true: a stored applied log has no such
            //element, and an equality with true never matches one
            Assert.Equal(new BsonDocument("$ne", false), RenderSameDayLogFilter()["IsApplied"]);
        }

        [Fact]
        public async Task TryIncrementSameDayUpdateLogAsync_WithNotAppliedUpdate_MatchesOnlyTheLogsStoredAsNotApplied()
        {
            // Action.
            await dbContext.OperationLogs.TryIncrementSameDayUpdateLogAsync(-1, Author, false, Reason, user);

            // Assert.
            //a "not true" would match also the applied logs, stored without the element
            Assert.Equal(BsonBoolean.False, RenderSameDayLogFilter()["IsApplied"]);
        }

        // Helpers.
        private BsonDocument RenderSameDayLogFilter()
        {
            Assert.NotNull(sameDayLogFilter);

            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            return sameDayLogFilter.Render(new RenderArgs<OperationLogBase>(
                new ModelMapSerializer<OperationLogBase>(dbContext.Engine),
                dbContext.Engine.SerializerRegistry));
        }
    }
}
