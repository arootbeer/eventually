using System;
using System.Collections.Generic;
using System.Linq;

namespace Eventually.Utilities.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool None<T>(this IEnumerable<T> input)
        {
            return input != null && !input.Any();
        }

        public static bool None<T>(this IEnumerable<T> input, Func<T, bool> predicate)
        {
            return input != null && !input.Any(predicate);
        }

        public static bool In<T>(this T input, params T[] candidates)
        {
            return input.In(candidates.ToHashSet());
        }

        public static bool In<T>(this T input, HashSet<T> candidates)
        {
            return candidates.Contains(input);
        }
    }
}