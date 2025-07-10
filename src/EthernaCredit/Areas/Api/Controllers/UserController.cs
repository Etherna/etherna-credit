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

using Etherna.CreditSystem.Areas.Api.DtoModels;
using Etherna.CreditSystem.Areas.Api.Services;
using Etherna.CreditSystem.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Api.Controllers
{
    [ApiController]
    [ApiVersion("0.3")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    public class UserController(IUserControllerService service) : ControllerBase
    {
        // GET.

        /// <summary>
        /// Get address for current user
        /// </summary>
        /// <returns>Ethereum address</returns>
        [HttpGet("address")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/json")] //force because of https://github.com/RicoSuter/NSwag/issues/4132
        public Task<string> GetAddressAsync() =>
            service.GetAddressAsync();

        /// <summary>
        /// Get credit status for current user
        /// </summary>
        /// <returns>Current credit status</returns>
        [HttpGet("credit")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<CreditDto> GetCreditAsync() =>
            service.GetCreditAsync();

        /// <summary>
        /// Get transaction logs for current user
        /// </summary>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">Current page on list</response>
        [HttpGet("logs")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<IEnumerable<OperationLogDto>> GetLogsAsync(
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            service.GetLogsAsync(page, take);
    }
}
