using System;
using System.Collections.Generic;

namespace Outkeep
{
    /// <summary>
    /// A rudimentary set of type system helpers.
    /// TODO: Refactor this into a proper service.
    /// </summary>
    internal static class TypeUtility
    {
        public static Type GetElementType(Type seqType)
        {
            var ienum = FindIEnumerable(seqType);

            return ienum is null ? seqType : ienum.GetGenericArguments()[0];
        }

        private static Type? FindIEnumerable(Type sequenceType)
        {
            if (sequenceType == null || sequenceType == typeof(string))
                return null;

            if (sequenceType.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(sequenceType.GetElementType());

            if (sequenceType.IsGenericType)
            {
                foreach (var arg in sequenceType.GetGenericArguments())
                {
                    var enumerable = typeof(IEnumerable<>).MakeGenericType(arg);

                    if (enumerable.IsAssignableFrom(sequenceType))
                    {
                        return enumerable;
                    }
                }
            }

            var interfaces = sequenceType.GetInterfaces();

            if (interfaces?.Length > 0)
            {
                foreach (var item in interfaces)
                {
                    var enumerable = FindIEnumerable(item);

                    if (enumerable != null) return enumerable;
                }
            }

            if (sequenceType.BaseType != null && sequenceType.BaseType != typeof(object))
            {
                return FindIEnumerable(sequenceType.BaseType);
            }

            return null;
        }
    }
}