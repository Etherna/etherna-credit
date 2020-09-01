using Etherna.EthernaCredit.Areas.Api.DtoModels;
using Etherna.EthernaCredit.Areas.Api.Services;
using Etherna.EthernaCredit.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.EthernaCredit.Areas.Api.Controllers
{
    [ApiController]
    [ApiVersion("0.2")]
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

        [HttpGet("credit")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<double> GetCreditAsync() =>
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
        public Task<IEnumerable<LogDto>> GetLogsAsync(
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            service.GetLogsAsync(page, take);
    }
}
