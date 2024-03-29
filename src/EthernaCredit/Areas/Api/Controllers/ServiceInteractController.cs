﻿//   Copyright 2021-present Etherna Sa
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
using Etherna.CreditSystem.Configs;
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
    [Authorize(CommonConsts.ServiceInteractApiScopePolicy)]
    public class ServiceInteractController : ControllerBase
    {
        // Fields.
        private readonly IServiceInteractControllerService service;

        // Constructor.
        public ServiceInteractController(
            IServiceInteractControllerService service)
        {
            this.service = service;
        }

        // Get.

        /// <summary>
        /// Get credit status for an user
        /// </summary>
        /// <param name="address">The user address</param>
        /// <returns>User credit</returns>
        [HttpGet("users/{address}/credit")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<CreditDto> GetUserCreditAsync([Required] string address) =>
            service.GetUserCreditAsync(address);

        /// <summary>
        /// Get logs generated by current service with a user
        /// </summary>
        /// <param name="address">The user address</param>
        /// <param name="fromDate">Low date limit for query</param>
        /// <param name="toDate">High date limit for query</param>
        /// <response code="200">Logs list from query</response>
        [HttpGet("users/{address}/oplogs")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<IEnumerable<OperationLogDto>> GetServiceOpLogsWithUserAsync(
            [Required] string address,
            DateTime? fromDate = null,
            DateTime? toDate = null) =>
            service.GetServiceOpLogsWithUserAsync(address, fromDate, toDate);

        // Put.

        /// <summary>
        /// Udpate the credit balance of an user by a given amount
        /// </summary>
        /// <param name="address">The user address</param>
        /// <param name="amount">The amount to be updated. Positive for credit, negative for debit</param>
        /// <param name="isApplied">True if credit update needs to be applied to user balance</param>
        /// <param name="reason">The update reason description</param>
        [HttpPut("users/{address}/credit/balance")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task RegisterBalanceUpdateAsync(
            [Required] string address,
            [Required] decimal amount,
            [Required] string reason,
            bool isApplied = true) =>
            service.RegisterBalanceUpdateAsync(address, amount, isApplied, reason);
    }
}
