using System.ComponentModel.DataAnnotations;

namespace Outkeep.Api.Http.V1.Models
{
    public class EchoRequest
    {
        [Required]
        [MaxLength(100)]
        public string Message { get; set; }
    }
}