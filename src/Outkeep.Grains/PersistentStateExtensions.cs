namespace Orleans.Runtime
{
    public static class PersistentStateExtensions
    {
        public static IPersistentState<TState> AsConflater<TState>(this IPersistentState<TState> state) where TState : new()
        {
            return new PersistentStateConflater<TState>(state);
        }
    }
}