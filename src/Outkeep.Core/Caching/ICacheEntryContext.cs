namespace Outkeep.Core.Caching
{
    internal interface ICacheEntryContext
    {
        public void OnPostEvictionCallbackRegistrationDisposed(PostEvictionCallbackRegistration registration);
    }
}