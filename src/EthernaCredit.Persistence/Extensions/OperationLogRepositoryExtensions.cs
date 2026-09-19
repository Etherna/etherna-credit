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
using Etherna.Credit.Domain.Models.OperationLogs;
using Etherna.MongoDB.Driver;
using Etherna.Scrinium.Core.Repositories;
using Etherna.SwarmSdk.Models;
using System;
using System.Threading.Tasks;

namespace Etherna.Credit.Persistence.Extensions
{
    public static class OperationLogRepositoryExtensions
    {
        // Methods.
        /// <summary>
        /// Add an amount to the service update log created today with the same author, applied flag, reason
        /// and user, so that a day of updates is kept by one cumulative log. The increment is atomic.
        /// </summary>
        /// <returns>True if a log of the day has been incremented, false if there is none yet</returns>
        public static async Task<bool> TryIncrementSameDayUpdateLogAsync(
            this IRepository<OperationLogBase, string> repository,
            XDaiValue amount,
            string author,
            bool isApplied,
            string reason,
            User user)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(user);

            var filterBuilder = Builders<UpdateOperationLog>.Filter;

            // The model map doesn't store IsApplied when it is true, its default value: an applied log must be
            // matched as "not false", because an equality with true never finds a stored document. A not applied
            // log always stores the element, and must not match the documents missing it.
            var isAppliedFilter = isApplied ?
                filterBuilder.Ne(log => log.IsApplied, false) :
                filterBuilder.Eq(log => log.IsApplied, false);

            var updatedLog = await repository.AccessToCollectionAsync(collection =>
                collection.FindOneAndUpdateAsync(
                    Builders<OperationLogBase>.Filter.OfType(
                        filterBuilder.Where(
                            log => log.Author == author &&
                                   log.CreationDateTime >= DateTime.Now.Date &&
                                   log.Reason == reason &&
                                   log.User.Id == user.Id) &
                        isAppliedFilter),
                    Builders<OperationLogBase>.Update.Inc(log => log.Amount, amount)));

            return updatedLog is not null;
        }
    }
}
