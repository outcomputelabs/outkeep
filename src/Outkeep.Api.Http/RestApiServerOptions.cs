using System;
using System.ComponentModel.DataAnnotations;

namespace Outkeep.Api.Http
{
    public class RestApiServerOptions
    {
        [Required]
        public Uri ApiUri { get; set; }

        [Required]
        public Uri SwaggerRelativeUri { get; set; } = new Uri("swagger", UriKind.Relative);

        [Required]
        public Uri SwaggerUiUri { get; set; }

        [Required]
        public string Title { get; set; } = nameof(Outkeep);

        [Required]
        public string VersionParameterName { get; set; } = "version";

        [Required]
        public bool RemoveVersionFromParameters { get; set; } = true;

        [Required]
        public bool ReplaceVersionParameterInPath { get; set; } = true;

        [Required]
        public bool EnableCompression { get; set; }
    }
}