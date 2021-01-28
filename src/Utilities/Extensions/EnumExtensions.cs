using System;

namespace Eventually.Utilities.Extensions
{
    public static class EnumExtensions
    {
        public static string GetName<TEnum>(this TEnum value) where TEnum : Enum
        {
            return Enum.GetName(typeof(TEnum), value);
        }
    }
}