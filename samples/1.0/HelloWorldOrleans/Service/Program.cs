using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using System.Threading.Tasks;

namespace Service
{
    internal class Program
    {
        private static Task Main()
        {
            return Host
                .CreateDefaultBuilder()
                .UseOrleans(orleans =>
                {
                    orleans.UseLocalhostClustering();
                })
                .RunConsoleAsync();
        }
    }
}