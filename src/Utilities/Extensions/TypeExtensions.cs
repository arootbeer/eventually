using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace Eventually.Utilities.Extensions
{
    public static class TypeExtensions
    {
        public static string GetFriendlyName(this Type type, bool useLongNames = false)
        {
            var friendlyName = useLongNames ? type.FullName : type.Name;
            if (type.IsGenericType)
            {
                friendlyName = GetTypeString(type, useLongNames);
            }
            return friendlyName;
        }

        private static string GetTypeString(Type type, bool useLongNames)
        {
            var t = (useLongNames ? type.AssemblyQualifiedName : type.Name) ?? throw new NullReferenceException();

            var output = new StringBuilder();

            var tickIndex = t.IndexOf('`') + 1;
            output.Append(t.Substring(0, tickIndex - 1).Replace("[", string.Empty));

            var genericTypes = type.GetGenericArguments();
            output.Append($"<{string.Join(",", genericTypes.Select(genType => genType.GetFriendlyName(useLongNames)))}>");
            return output.ToString();
        }

        public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return baseType.IsAssignableToGenericType(genericType);
        }

        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, Type>> _derivedInterfaceCache =
            new ConcurrentDictionary<Type, ConcurrentDictionary<Type, Type>>();

        public static Type GetMostDerivedImplementationType<TInterface>(this object input)
        {
            var baseType = typeof(TInterface);
            return input.GetMostDerivedImplementationType(baseType);
        }

        public static Type GetMostDerivedImplementationType(this object input, Type baseType)
        {
            var inputType = input is Type t ? t : input.GetType();
            if (inputType == baseType)
            {
                return inputType;
            }

            if (_derivedInterfaceCache.ContainsKey(inputType) && _derivedInterfaceCache[inputType].ContainsKey(baseType))
            {
                return _derivedInterfaceCache[inputType][baseType];
            }

            var interfaces = inputType.GetInterfaces().ToList();
            interfaces = interfaces
                .Except(interfaces.SelectMany(iface => iface.GetInterfaces()))
                .Where(baseType.IsAssignableFrom)
                .OrderByDescending(type => type.GetInterfaces().Length)
                .ToList();

            var implementation = interfaces.FirstOrDefault(baseType.IsAssignableFrom);
            return implementation;
        }
    }
}