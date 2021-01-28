using System.Collections;

namespace Eventually.Utilities.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue Get<TKey, TValue>(this IDictionary input, TKey key)
        {
            return (TValue)input[key];
        }
    }
}