using Microsoft.AspNetCore.Mvc;
using Orleans;
using Orleans.Runtime;
using Outkeep.Api.Http.V1.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Outkeep.Api.Http.V1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EchoController : ControllerBase
    {
        private readonly IGrainFactory factory;

        public EchoController(IGrainFactory factory)
        {
            this.factory = factory;
        }

        [HttpGet]
        [SwaggerOperation(OperationId = "Echo")]
        public async Task<ActionResult<EchoResponse>> GetAsync([FromQuery] EchoRequest model)
        {
            Contract.Requires(model != null);

            var reply = await factory
                .GetEchoGrain()
                .EchoAsync(model.Message)
                .ConfigureAwait(false);

            return Ok(new EchoResponse
            {
                ActivityId = RequestContext.ActivityId,
                Message = reply
            });
        }
    }
}