using System;
using System.Collections.Generic;
using System.Linq;

namespace Eventually.Tests.Core
{
    public static class TestExtensions
    {
        private static readonly Random Random = new Random();

        public static IEnumerable<Type> GetBaseTypes(this Type type, bool includeInterfaces = false)
        {
            if (type == null)
                yield break;

            for (var nextType = type.BaseType; nextType != null; nextType = nextType.BaseType)
                yield return nextType;

            if (!includeInterfaces)
                yield break;

            foreach (var i in type.GetInterfaces())
                yield return i;
        }

        public static IEnumerable<T> RandomSwap<T>(this IEnumerable<T> input)
        {
            var values = input.ToArray();
            var indexes = Enumerable.Range(0, values.Length).ToList();
            var result = new List<T>(values.Length);
            while (indexes.Count > 0)
            {
                var index = indexes[Random.Next(indexes.Count)];
                result.Add(values[index]);
                indexes.Remove(index);
            }

            return result.AsReadOnly();
        }

        public static string Randomize(this string input)
        {
            return new string(input.RandomSwap().ToArray());
        }

        public static T RandomValue<T>(this IEnumerable<T> candidates)
        {
            var values = candidates.ToArray();
            return values[Random.Next(values.Length)];
        }
    }
}