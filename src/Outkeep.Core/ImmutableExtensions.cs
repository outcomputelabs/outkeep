namespace Orleans.Concurrency
{
    public static class ImmutableExtensions
    {
        public static Immutable<T?> AsNullableImmutable<T>(this T? value) where T : class
        {
            return new Immutable<T?>(value);
        }
    }
}