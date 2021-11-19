﻿using Etherna.Authentication.Extensions;
using Etherna.CreditSystem.Areas.Api.DtoModels;
using Etherna.CreditSystem.Areas.Api.Services;
using Etherna.CreditSystem.Attributes;
using Etherna.CreditSystem.Configs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        // Put.

        /// <summary>
        /// Udpate the credit balance of an user by a given ammount
        /// </summary>
        /// <param name="address">The user address</param>
        /// <param name="ammount">The ammount to be updated. Positive for credit, negative for debit</param>
        /// <param name="reason">The update reason description</param>
        [HttpPut("users/{address}/credit/balance")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task RegisterBalanceUpdateAsync([Required] string address, [Required]double ammount, [Required]string reason) =>
            service.RegisterBalanceUpdateAsync(User.GetClientId(), address, ammount, reason);
    }
}
