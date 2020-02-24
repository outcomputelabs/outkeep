using System;
using System.ComponentModel.DataAnnotations;

namespace Outkeep.Api.Http
{
    /// <summary>
    /// Options for configuring the <see cref="OutkeepHttpApiHostedService"/> instance.
    /// </summary>
    public class OutkeepHttpApiServerOptions
    {
        /// <summary>
        /// The URI for the Outkeep HTTP API to listen on.
        /// </summary>
        [Required]
        public Uri? ApiUri { get; set; }

        /// <summary>
        /// The user interface title for the Outkeep HTTP API instance.
        /// </summary>
        [Required]
        public string Title { get; set; } = nameof(Outkeep);

        public long? MaxRequestBodySize { get; set; }
    }
}