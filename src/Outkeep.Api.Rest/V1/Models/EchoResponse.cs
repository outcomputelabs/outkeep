using System;

namespace Outkeep.Api.Http.V1.Models
{
    public class EchoResponse
    {
        public Guid ActivityId { get; set; }
        public string Message { get; set; }
    }
}