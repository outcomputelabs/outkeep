using System;

namespace Outkeep.Api.Http.Models.V1
{
    /// <summary>
    /// The response from an echo request.
    /// </summary>
    public class Echo
    {
        /// <summary>
        /// Correlation identifier for the request.
        /// </summary>
        public Guid ActivityId { get; set; }

        /// <summary>
        /// The message present in the request.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// The API version that fulfilled the request.
        /// </summary>
        public string? Version { get; set; }
    }
}