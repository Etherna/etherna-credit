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

using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Core.Clusters;
using Etherna.MongoDB.Driver.Core.Connections;
using Etherna.MongoDB.Driver.Core.Servers;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.Credit.Areas.Api
{
    public class ExceptionHandlerTest
    {
        // Tests.
        [Fact]
        public async Task RunAsync_WithTransientTransactionError_AnswersConflict()
        {
            // Arrange.
            //a write conflict between concurrent transactions: the server asks the client to retry
            var exception = BuildWriteConflictException();
            exception.AddErrorLabel("TransientTransactionError");

            // Action.
            var result = await ExceptionHandler.RunAsync(() => throw exception);

            // Assert.
            Assert.Equal(StatusCodes.Status409Conflict, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        }

        [Fact]
        public async Task RunAsync_WithUnlabeledDriverException_AnswersInternalServerError()
        {
            // Arrange.
            //the same driver exception without the transient label is not a retryable conflict
            var exception = BuildWriteConflictException();

            // Action.
            var result = await ExceptionHandler.RunAsync(() => throw exception);

            // Assert.
            Assert.Equal(StatusCodes.Status500InternalServerError, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        }

        // Helpers.
        private static MongoCommandException BuildWriteConflictException()
        {
            var connectionId = new ConnectionId(new ServerId(new ClusterId(), new DnsEndPoint("localhost", 27017)));
            var command = new BsonDocument("findAndModify", "logs");
            var result = new BsonDocument
            {
                { "ok", 0 },
                { "errmsg", "Write conflict during plan execution and yielding is disabled" },
                { "code", 112 },
                { "codeName", "WriteConflict" }
            };
            return new MongoCommandException(connectionId, "Command findAndModify failed", command, result);
        }
    }
}
