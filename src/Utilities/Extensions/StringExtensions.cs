using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eventually.Utilities.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhitespace(this string input)
        {
            return string.IsNullOrWhiteSpace(input);
        }

        public static bool IsNotNullOrWhitespace(this string input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }

        public static byte[] ToASCIIByteArray(this string input)
        {
            return input.ToByteArray(Encoding.ASCII);
        }

        public static byte[] ToByteArray(this string input, Encoding encoding)
        {
            return encoding.GetBytes(input);
        }

        public static string TrimAndPadLeftWithTotalLength(this string input, int length, char paddingChar = ' ')
        {
            return (input ?? "").Trim()
                .PadLeft(length, paddingChar)
                .Substring(0, length);
        }

        public static string TrimAndPadRightWithTotalLength(this string input, int length, char paddingChar = ' ')
        {
            return (input ?? "").Trim()
                .PadRight(length, paddingChar)
                .Substring(0, length);
        }

        public static string FormatWithConjunction(this IEnumerable<string> input, string conjunction)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (conjunction.IsNullOrWhitespace())
            {
                throw new ArgumentException($"`{conjunction ?? "<null>"}` is not a valid conjunction");
            }

            conjunction = conjunction.Trim();
            input = input.Where(s => s.IsNotNullOrWhitespace()).ToList();

            if (!input.Any())
            {
                throw new Exception($"{nameof(FormatWithConjunction)} cannot be called with an empty input or only null or whitespace strings");
            }

            if (input.Count() == 1)
            {
                return input.First();
            }

            if (input.Count() == 2)
            {
                return $"{input.First()} {conjunction} {input.Last()}";
            }

            var commaSeparatedInput = string.Join(", ", input);
            return commaSeparatedInput.Insert(commaSeparatedInput.LastIndexOf(", ", StringComparison.Ordinal) + 2, $"{conjunction} ");
        }
    }
}