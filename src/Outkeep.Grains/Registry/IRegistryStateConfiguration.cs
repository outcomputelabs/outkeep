namespace Outkeep.Registry
{
    internal interface IRegistryStateConfiguration
    {
        public string? StorageName { get; }
        public string? ContainerName { get; }
        public string? RegistryName { get; }
    }
}