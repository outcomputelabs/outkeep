using Microsoft.AspNetCore.Mvc;
using Orleans;
using Orleans.Concurrency;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Outkeep.Api.Http.Controllers.V1
{
    /// <summary>
    /// Basic binary object cache to support ASP.NET
    /// </summary>
    [ApiController]
    [ApiVersion("1")]
    [ControllerName("cache")]
    [Route("api/v{version:apiVersion}/cache")]
    public class CacheController : ControllerBase
    {
        private readonly IGrainFactory factory;

        public CacheController(IGrainFactory factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// Gets a cache item by its key
        /// </summary>
        /// <param name="key">The cache key</param>
        [HttpGet]
        [SwaggerOperation(OperationId = "GetCache")]
        [Route("{key}")]
        public async Task<ActionResult<byte[]>> GetAsync([FromRoute] [Required] [MaxLength(128)] string key)
        {
            var reply = await factory
                .GetCacheGrain(key)
                .GetAsync()
                .ConfigureAwait(false);

            if (reply.Value == null)
            {
                return NoContent();
            }

            return Ok(reply.Value);
        }

        [HttpPost]
        [SwaggerOperation(OperationId = "SetCache")]
        [Route("{key}")]
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "N/A")]
        public async Task<ActionResult> SetAsync(
            [FromRoute] [Required] [MaxLength(128)] string key,
            [FromForm] [Required] byte[] value,
            [FromForm] DateTimeOffset? absoluteExpiration,
            [FromForm] TimeSpan? slidingExpiration)
        {
            await factory
                .GetCacheGrain(key)
                .SetAsync(value.AsImmutable(), absoluteExpiration, slidingExpiration)
                .ConfigureAwait(false);

            return Ok();
        }
    }
}