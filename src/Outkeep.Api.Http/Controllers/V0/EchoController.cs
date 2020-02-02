using Microsoft.AspNetCore.Mvc;
using Orleans;
using Orleans.Runtime;
using Outkeep.Api.Http.Models.V0;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Outkeep.Api.Http.Controllers.V0
{
    /// <summary>
    /// Echoes back messages to facilitate basic connectivity testing.
    /// </summary>
    [ApiController]
    [ApiVersion("0")]
    [Route("api/v{version:apiVersion}/echo")]
    public class EchoController : ControllerBase
    {
        private readonly IGrainFactory factory;

        /// <summary>
        /// Creates a new instance of <see cref="EchoController"/>.
        /// </summary>
        public EchoController(IGrainFactory factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// Echoes back the given message along with the api version that fulfilled the request.
        /// For testing basic connectivity and routing to the API.
        /// </summary>
        /// <param name="message">The message to echo.</param>
        /// <returns>Returns the input message along with the api version that fulfilled the request.</returns>
        [HttpGet]
        [SwaggerOperation(OperationId = "echo")]
        public async Task<ActionResult<Echo>> GetAsync(
            [Required] [MaxLength(100)] string message)
        {
            var reply = await factory
                .GetEchoGrain()
                .EchoAsync(message)
                .ConfigureAwait(false);

            return Ok(new Echo
            {
                ActivityId = RequestContext.ActivityId,
                Message = reply,
                Version = "0"
            });
        }
    }
}