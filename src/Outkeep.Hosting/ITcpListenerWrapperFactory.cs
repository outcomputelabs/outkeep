namespace Outkeep.Hosting
{
    public interface ITcpListenerWrapperFactory
    {
        ITcpListenerWrapper Create(int port, bool exclusiveAddressUse);
    }
}