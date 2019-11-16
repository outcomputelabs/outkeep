using Microsoft.AspNetCore.Http;
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
        [SwaggerResponse(StatusCodes.Status200OK, Description = "Cache entry retrieved")]
        [SwaggerResponse(StatusCodes.Status204NoContent, Description = "Cache entry not found")]
        public async Task<ActionResult<byte[]>> GetAsync(
            [Required] [MaxLength(128)] string key)
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

        /// <summary>
        /// Caches a value with the given key
        /// </summary>
        /// <param name="key">The key to identify the value with</param>
        /// <param name="absoluteExpiration">The absolute date and time at which the value will expire</param>
        /// <param name="slidingExpiration">The sliding time span at which the value will expire if not accessed</param>
        /// <param name="value">The value to cache</param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation(OperationId = "SetCache")]
        [Route("{key}")]
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "N/A")]
        public async Task<ActionResult> SetAsync(
            [Required] [MaxLength(128)] string key,
            DateTimeOffset? absoluteExpiration,
            TimeSpan? slidingExpiration,
            [Required] IFormFile value)
        {
            var bytes = new byte[value.Length];
            using (var stream = value.OpenReadStream())
            {
                await stream.ReadAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            }

            await factory
                .GetCacheGrain(key)
                .SetAsync(bytes.AsImmutable(), absoluteExpiration, slidingExpiration)
                .ConfigureAwait(false);

            return Ok();
        }

        /// <summary>
        /// Removes a cache entry
        /// </summary>
        /// <param name="key">The key of the entry to remove</param>
        [HttpDelete]
        [SwaggerOperation(OperationId = "RemoveCache")]
        [Route("{key}")]
        [SwaggerResponse(StatusCodes.Status200OK, Description = "entry removed")]
        public async Task<ActionResult> RemoveAsync(
            [Required] [MaxLength(128)] string key)
        {
            await factory
                .GetCacheGrain(key)
                .RemoveAsync()
                .ConfigureAwait(false);

            return Ok();
        }
    }
}