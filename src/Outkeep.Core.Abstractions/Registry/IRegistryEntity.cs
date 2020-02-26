namespace Outkeep.Registry
{
    public interface IRegistryEntity<TState>
    {
        public string Key { get; }
        public TState State { get; set; }
        public string Etag { get; set; }
    }
}