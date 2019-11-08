using Microsoft.AspNetCore.Mvc;
using Outkeep.Api.Rest.V1.Models;
using Outkeep.Client;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace Outkeep.Api.Rest.V1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EchoController : ControllerBase
    {
        private readonly IOutkeepClient client;

        public EchoController(IOutkeepClient client)
        {
            this.client = client;
        }

        [HttpGet]
        [SwaggerOperation(OperationId = "Echo")]
        public async Task<ActionResult<EchoResponse>> GetAsync([FromQuery] EchoRequest model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var reply = await client.GetEchoGrain()
                .EchoAsync(model.Message)
                .ConfigureAwait(false);

            return Ok(new EchoResponse
            {
                ActivityId = client.ActivityId,
                Message = reply
            });
        }
    }
}