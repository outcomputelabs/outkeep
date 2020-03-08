namespace Outkeep.Caching
{
    public delegate T CreateStateAction<out T>(string key) where T : ICacheRegistryEntryState;
}