using System.Globalization;

namespace System
{
    public static class StringExtensions
    {
        public static string Format(this string format, object arg0)
        {
            return string.Format(CultureInfo.InvariantCulture, format, arg0);
        }

        public static string Format(this string format, object arg0, object arg1)
        {
            return string.Format(CultureInfo.InvariantCulture, format, arg0, arg1);
        }

        public static string Format(this string format, object arg0, object arg1, object arg2)
        {
            return string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2);
        }
    }
}