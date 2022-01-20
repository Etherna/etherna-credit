//   Copyright 2021-present Etherna Sagl
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

using Etherna.CreditSystem.Areas.Api.DtoModels;
using Etherna.CreditSystem.Areas.Api.Services;
using Etherna.CreditSystem.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.CreditSystem.Areas.Api.Controllers
{
    [ApiController]
    [ApiVersion("0.3")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        // Fields.
        private readonly IUserControllerService service;

        // Constructors.
        public UserController(IUserControllerService service)
        {
            this.service = service;
        }

        // GET.

        /// <summary>
        /// Get address for current user
        /// </summary>
        /// <returns>Ethereum address</returns>
        [HttpGet("address")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public string GetAddress() =>
            service.GetAddress(User);

        /// <summary>
        /// Get credit status for current user
        /// </summary>
        /// <returns>Current credit status</returns>
        [HttpGet("credit")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<CreditDto> GetCreditAsync() =>
            service.GetCreditAsync(User);

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
            service.GetLogsAsync(User, page, take);
    }
}
