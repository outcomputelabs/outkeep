using System;
using System.ComponentModel.DataAnnotations;

namespace Outkeep.Api.Http
{
    public class OutkeepHttpApiServerOptions
    {
        [Required]
        public Uri ApiUri { get; set; }

        [Required]
        public string Title { get; set; } = nameof(Outkeep);
    }
}