using Microsoft.Extensions.Hosting;

namespace Outkeep.Server
{
    public class OutkeepServerBuilder : IOutkeepServerBuilder
    {
        private readonly IHostBuilder builder;

        public OutkeepServerBuilder(IHostBuilder builder)
        {
            this.builder = builder;
        }
    }
}