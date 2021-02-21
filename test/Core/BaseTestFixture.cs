using System;
using System.Linq;
using Eventually.Utilities.Extensions;

namespace Eventually.Tests.Core
{
    public abstract class BaseTestFixture
    {
        private static readonly Random Random = new();

        /// <summary>
        /// Returns a random long value (between the min and/or max values, if provided)
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static long RandomLong(long min = long.MinValue, long max = long.MaxValue)
        {
            if (min > max)
            {
                throw new ArgumentException($"min value `{min}` may not be greater than max value `{max}`");
            }

            if (min == max)
            {
                return min;
            }

            var mask = max | min;
            return mask & BitConverter.ToInt64(Guid.NewGuid().ToByteArray().Take(8).ToArray(), 0);
        }

        /// <summary>
        /// Returns a random int value (between the min and/or max values, if provided)
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int RandomInt(int min = int.MinValue, int max = int.MaxValue)
        {
            if (min > max)
            {
                throw new ArgumentException($"min value `{min}` may not be greater than max value `{max}`");
            }

            if (min == max)
            {
                return min;
            }

            var mask = max | min;
            return mask & BitConverter.ToInt32(Guid.NewGuid().ToByteArray().Take(8).ToArray(), 0);
        }

        /// <summary>
        /// Returns a string of characters of the specified length comprised of a random selection from the specified valid characters
        /// </summary>
        /// <param name="length">The length of the returned string</param>
        /// <param name="validCharacters">The characters which may be selected for inclusion in the returned string (if not provided, the set of alphanumeric characters [A-Za-z0-9] is used)</param>
        /// <returns></returns>
        public static string RandomString(int length, params char[] validCharacters)
        {
            if (length < 0)
            {
                throw new ArgumentException($"Length value `{length}` must be non-negative");
            }

            if (validCharacters.None())
            {
                validCharacters = AlphaNumericCharacters;
            }

            var chars = Enumerable.Range(0, length)
                .Select(i => validCharacters[Random.Next(validCharacters.Length)])
                .ToArray();
            
            return new string(chars);
        }

        /// <summary>
        /// Returns a string of characters of the specified length comprised of a random selection from the specified valid characters
        /// </summary>
        /// <param name="minLength">The minimum length of the returned string</param>
        /// <param name="maxLength">The maximum length of the returned string</param>
        /// <param name="validCharacters">The characters which may be selected for inclusion in the returned string (if not provided, the set of alphanumeric characters [A-Za-z0-9] is used)</param>
        /// <returns></returns>
        public static string RandomString(int minLength, int maxLength, params char[] validCharacters)
        {
            if (minLength < 0 || maxLength < minLength)
            {
                throw new ArgumentException($"minLength value `{minLength}` must be non-negative and not greater than maxLength value `{maxLength}`");
            }

            return RandomString(Random.Next(minLength, maxLength), validCharacters);
        }

        public static char[] UpperAlphaCharacters { get; } = Enumerable.Range(65, 26).Select(i => (char) i).ToArray();

        public static char[] LowerAlphaCharacters { get; } = Enumerable.Range(97, 26).Select(i => (char) i).ToArray();

        public static char[] DigitCharacters { get; } = Enumerable.Range(48, 10).Select(i => (char) i).ToArray();

        public static char[] AlphaNumericCharacters { get; } = UpperAlphaCharacters.Concat(LowerAlphaCharacters).Concat(DigitCharacters).ToArray();

        public static char[] PrintableCharacters { get; } = Enumerable.Range(32, 94).Select(i => (char) i).ToArray();
    }
}