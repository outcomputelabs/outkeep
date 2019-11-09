using System;
using System.ComponentModel.DataAnnotations;

namespace Outkeep.Api.Rest
{
    public class RestApiServerOptions
    {
        [Required]
        public Uri ApiUri { get; set; }

        [Required]
        public Uri SwaggerRelativeUri { get; set; } = new Uri("swagger", UriKind.Relative);

        [Required]
        public Uri SwaggerUiUri { get; set; }
    }
}