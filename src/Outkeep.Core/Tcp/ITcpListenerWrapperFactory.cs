namespace Outkeep.Tcp
{
    public interface ITcpListenerWrapperFactory
    {
        ITcpListenerWrapper Create(int port, bool exclusiveAddressUse);
    }
}