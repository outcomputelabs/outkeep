using System.ComponentModel.DataAnnotations;

namespace Outkeep.Api.Http.Models
{
    public class EchoRequest
    {
        [Required]
        [MaxLength(100)]
        public string Message { get; set; }
    }
}