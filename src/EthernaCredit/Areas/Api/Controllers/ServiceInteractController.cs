using Etherna.EthernaCredit.Areas.Api.Services;
using Etherna.EthernaCredit.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.EthernaCredit.Areas.Api.Controllers
{
    [ApiController]
    [ApiVersion("0.2")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    [Authorize("ServiceInteractApiScope")]
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
        /// Get user balance from a provided user address
        /// </summary>
        /// <param name="address">The user address</param>
        /// <returns>User balance</returns>
        [HttpGet("users/{address}/balance")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<double> GetUserBalanceAsync([Required] string address) =>
            service.GetUserBalanceAsync(address);

        // Put.

        /// <summary>
        /// Udpate balance of an user by a given ammount
        /// </summary>
        /// <param name="address">The user address</param>
        /// <param name="ammount">The ammount to be updated. Positive for credit, negative for debit</param>
        /// <param name="reason">The update reason description</param>
        [HttpPut("users/{address}/balance")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task RegisterBalanceUpdateAsync([Required] string address, [Required]double ammount, [Required]string reason) =>
            service.RegisterBalanceUpdateAsync(address, ammount, reason);
    }
}
