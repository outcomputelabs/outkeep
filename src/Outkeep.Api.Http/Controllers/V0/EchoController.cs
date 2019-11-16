using Microsoft.AspNetCore.Mvc;
using Orleans;
using Orleans.Runtime;
using Outkeep.Api.Http.Models.V0;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Outkeep.Api.Http.Controllers.V0
{
    [ApiController]
    [ApiVersion("0")]
    [Route("api/v{version:apiVersion}/echo")]
    public class EchoController : ControllerBase
    {
        private readonly IGrainFactory factory;

        public EchoController(IGrainFactory factory)
        {
            this.factory = factory;
        }

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