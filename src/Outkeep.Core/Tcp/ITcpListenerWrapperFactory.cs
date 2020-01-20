namespace Outkeep.Core.Tcp
{
    public interface ITcpListenerWrapperFactory
    {
        ITcpListenerWrapper Create(int port, bool exclusiveAddressUse);
    }
}