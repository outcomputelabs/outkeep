using Microsoft.AspNetCore.Mvc;
using Orleans;
using Orleans.Runtime;
using Outkeep.Api.Http.Models.V1;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace Outkeep.Api.Http.Controllers.V1
{
    [ApiController]
    [ApiVersion("1")]
    [ControllerName("echo")]
    [Route("api/v{version:apiVersion}/echo")]
    public class EchoController : ControllerBase
    {
        private readonly IGrainFactory factory;

        public EchoController(IGrainFactory factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// Echoes back the given message along with the api version that fulfilled the request.
        /// For testing basic connectivity and routing to the API.
        /// </summary>
        /// <param name="model.Message">qqq</param>
        /// <returns>Returns the input message along with the api version that fulfilled the request.</returns>
        [HttpGet]
        [SwaggerOperation(OperationId = "Echo")]
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
                Version = "1"
            });
        }
    }
}