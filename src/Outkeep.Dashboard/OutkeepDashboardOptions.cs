using System;
using System.Diagnostics.CodeAnalysis;

namespace Outkeep.Dashboard
{
    public class OutkeepDashboardOptions
    {
        [SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "Default")]
        public Uri Url { get; set; } = new Uri("http://localhost:5001");
    }
}