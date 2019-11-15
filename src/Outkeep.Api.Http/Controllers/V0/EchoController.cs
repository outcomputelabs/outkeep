using Microsoft.AspNetCore.Mvc;
using Orleans;
using Orleans.Runtime;
using Outkeep.Api.Http.Models.V0;
using Swashbuckle.AspNetCore.Annotations;
using System;
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
        public async Task<ActionResult<EchoResponse>> GetAsync([FromQuery] EchoRequest model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var reply = await factory
                .GetEchoGrain()
                .EchoAsync(model.Message)
                .ConfigureAwait(false);

            return Ok(new EchoResponse
            {
                ActivityId = RequestContext.ActivityId,
                Message = reply,
                Version = "0"
            });
        }
    }
}