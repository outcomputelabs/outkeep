using System;
using System.Diagnostics.CodeAnalysis;

namespace Outkeep.Dashboard
{
    public class OutkeepDashboardOptions
    {
        [SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "Default")]
        public Uri Url { get; set; } = new Uri("http://localhost:5001");

        /// <summary>
        /// Title to show in the dashboard.
        /// Use the name of your application here.
        /// </summary>
        public string? Brand { get; set; }
    }
}