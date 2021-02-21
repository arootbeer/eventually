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
    }
}