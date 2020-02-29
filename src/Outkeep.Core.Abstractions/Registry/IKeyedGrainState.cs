using Orleans;

namespace Outkeep.Registry
{
    public interface IKeyedGrainState : IGrainState
    {
        public string Key { get; }
    }
}