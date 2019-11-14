using System;

namespace Outkeep.Api.Http.Models
{
    public class EchoResponse
    {
        public Guid ActivityId { get; set; }
        public string Message { get; set; }
        public string Version { get; set; }
    }
}