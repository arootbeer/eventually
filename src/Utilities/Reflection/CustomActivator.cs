//based on https://github.com/fabriciorissetto/CustomActivator/blob/master/ReflectionHelper/CustomActivator.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Fasterflect;
using ImpromptuInterface;

namespace Eventually.Utilities.Reflection
{
    public static class CustomActivator
    {
        public static TObject CreateInstance<TObject>() where TObject : class
        {
            return CreateInstanceAndPopulateProperties<TObject>(null);
        }

        public static TObject CreateInstanceAndPopulateProperties<TObject>(object valueProvider) where TObject : class
        {
            if (valueProvider == null)
            {
                return CreateInstance<TObject>();
            }

            if (!(valueProvider is IDictionary<string, object> propertyValues))
            {
                propertyValues = TypeDescriptor.GetProperties(valueProvider)
                    .Cast<PropertyDescriptor>()
                    .ToDictionary(pd => pd.Name, pd => pd.GetValue(valueProvider));
            }

            return CreateInstanceAndPopulateProperties<TObject>(propertyValues);
        }

        public static TObject CreateInstanceAndPopulateProperties<TObject>(IDictionary<string, object> propertyValues) where TObject : class
        {
            var desiredType = typeof(TObject);
            TObject result;

            if (desiredType.IsInterface)
            {
                // The "Activator.CreateInstance<>" from the .NET Framework can't instantiate interfaces
                // To do this we need to create an object (in runtime) that implements this interface
                result = CreateInterfaceInstance<TObject>(propertyValues);
            }
            else
            {
                result = (TObject) FormatterServices.GetUninitializedObject(typeof(TObject));
                foreach (var propertyValuePair in propertyValues)
                {
                    if (!result.TrySetPropertyValue(propertyValuePair.Key, propertyValuePair.Value))
                    {
                        result.SetFieldValue($"<{propertyValuePair.Key}>k__BackingField", propertyValuePair.Value);
                    }
                }
            }

            return result;
        }

        private static TInterface CreateInterfaceInstance<TInterface>(IDictionary<string, object> propertiesValues = null) where TInterface : class
        {
            dynamic expandoObject = new ExpandoObject();

            var expandoType = typeof(TInterface);
            var expandoProperties = expandoObject as IDictionary<string, object>;

            foreach (var destinationProperty in GetPublicProperties(expandoType))
            {
                // Get value from dictionary if it exists
                if (propertiesValues != null && propertiesValues.ContainsKey(destinationProperty.Name))
                {
                    var parameterValue = propertiesValues[destinationProperty.Name];
                    var convertedValue = CastValueToPropertyType(destinationProperty, parameterValue);
                    expandoProperties[destinationProperty.Name] = convertedValue;
                }
                // If not, get the default value
                else
                {
                    var propertyType = destinationProperty.PropertyType;
                    var defaultValue = propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;
                    expandoProperties[destinationProperty.Name] = defaultValue;
                }
            }

            return Impromptu.ActLike<TInterface>(expandoObject);
        }

        private static readonly MethodInfo CreateTypedEnumerableMethod = new Func<IEnumerable, IEnumerable<object>>(CreateEnumerableInstance<object>)
            .Method
            .GetGenericMethodDefinition();
        private static object CastValueToPropertyType(PropertyInfo destinationProperty, object value)
        {
            var propertyType = destinationProperty.PropertyType;
            if (value == null)
            {
                return propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;
            }

            if (propertyType.IsInstanceOfType(value))
            {
                return value;
            }

            if (typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType.IsConstructedGenericType)
            {
                var collectionType = propertyType.GetGenericArguments()[0];
                var createMethod = CreateTypedEnumerableMethod.MakeGenericMethod(collectionType);
                return createMethod.Invoke(null, new[] {value});
            }

            var converter = TypeDescriptor.GetConverter(propertyType);
            return converter.ConvertFrom(value);
        }

        private static IEnumerable<T> CreateEnumerableInstance<T>(IEnumerable untypedValues) where T : class
        {
            return untypedValues
                .Cast<object>()
                .Select(CreateInstanceAndPopulateProperties<T>)
                .ToList();
        }

        private static PropertyInfo[] GetPublicProperties(this Type type)
        {
            if (type.IsInterface)
            {
                var propertyInfos = new List<PropertyInfo>();

                var considered = new List<Type>();
                var queue = new Queue<Type>();
                considered.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    var subType = queue.Dequeue();
                    foreach (var subInterface in subType.GetInterfaces())
                    {
                        if (considered.Contains(subInterface)) continue;

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

                    var typeProperties = subType.GetProperties(
                        BindingFlags.FlattenHierarchy
                        | BindingFlags.Public
                        | BindingFlags.Instance);

                    var newPropertyInfos = typeProperties
                        .Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToArray();
            }

            return type.GetProperties(BindingFlags.FlattenHierarchy
                | BindingFlags.Public | BindingFlags.Instance);
        }
    }
}