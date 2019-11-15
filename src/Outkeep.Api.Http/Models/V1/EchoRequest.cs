using System.ComponentModel.DataAnnotations;

namespace Outkeep.Api.Http.Models.V1
{
    /// <summary>
    /// Represents a request to echo a message back.
    /// </summary>
    public class EchoRequest
    {
        /// <summary>
        /// The message to echo back.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Message { get; set; }
    }
}