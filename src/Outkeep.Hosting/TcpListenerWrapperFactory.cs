namespace Outkeep.Hosting
{
    /// <summary>
    /// Default implementation of <see cref="ITcpListenerWrapperFactory"/>.
    /// </summary>
    public sealed class TcpListenerWrapperFactory : ITcpListenerWrapperFactory
    {
        private TcpListenerWrapperFactory()
        {
        }

        public ITcpListenerWrapper Create(int port, bool exclusiveAddressUse)
        {
            return new TcpListenerWrapper(port, exclusiveAddressUse);
        }

        public static readonly TcpListenerWrapperFactory Default = new TcpListenerWrapperFactory();
    }
}