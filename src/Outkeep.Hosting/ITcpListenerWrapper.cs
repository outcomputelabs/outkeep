using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Outkeep.Hosting
{
    /// <summary>
    /// Wraps static calls to the <see cref="TcpListener"/> class to facilitate testing.
    /// </summary>
    public interface ITcpListenerWrapper
    {
        void Start();

        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Wrapper")]
        void Stop();

        EndPoint LocalEndpoint { get; }
    }
}