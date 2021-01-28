using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Fasterflect;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace Eventually.Utilities.Serialization
{
    public class ImmutableTypesJsonSerializer
    {
        private class NonPublicPropertiesResolver : DefaultContractResolver
        {
            protected override JsonObjectContract CreateObjectContract(Type objectType)
            {
                var contract = base.CreateObjectContract(objectType);
                contract.DefaultCreator = () => FormatterServices.GetUninitializedObject(objectType);
                return contract;
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var prop = base.CreateProperty(member, memberSerialization);
                if (member is PropertyInfo pi)
                {
                    prop.Readable = (pi.GetMethod != null);
                    prop.Writable = (true);
                }
                return prop;
            }

            protected override IValueProvider CreateMemberValueProvider(MemberInfo member)
            {
                return new GetOnlyReflectionValueProvider(member);
            }
        }

        private class GetOnlyReflectionValueProvider : IValueProvider
        {
            private readonly MemberInfo _memberInfo;
            private readonly bool _isGetOnlyProperty;
            private readonly IValueProvider _reflectionValueProvider;

            public GetOnlyReflectionValueProvider(MemberInfo memberInfo)
            {
                _memberInfo = memberInfo;
                _isGetOnlyProperty = memberInfo is PropertyInfo pi && pi.SetMethod == null;

                _reflectionValueProvider = new ReflectionValueProvider(memberInfo);
            }

            public void SetValue(object target, object value)
            {
                if (!_isGetOnlyProperty)
                {
                    _reflectionValueProvider.SetValue(target, value);
                    return;
                }

                var backingFieldName = $"<{_memberInfo.Name}>k__BackingField";
                target.SetFieldValue(backingFieldName, value);
            }

            public object GetValue(object target)
            {
                return _reflectionValueProvider.GetValue(target);
            }
        }

        private readonly IEnumerable<Type> _knownTypes = new[] {typeof(Dictionary<string, object>)};

        private readonly JsonSerializer _typedSerializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.All,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new NonPublicPropertiesResolver()
            }
            .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

        private readonly JsonSerializer _untypedSerializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.Auto,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new NonPublicPropertiesResolver(),
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            }
            .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

        public ImmutableTypesJsonSerializer(params Type[] knownTypes)
        {
            if (knownTypes != null)
            {
                _knownTypes = _knownTypes.Concat(knownTypes).ToArray();
            }
        }

        public virtual void Serialize<T>(Stream output, T graph)
        {
            using var streamWriter = new StreamWriter(output, Encoding.UTF8);
            using var contentWriter = new StringWriter();
            Serialize(new JsonTextWriter(contentWriter), graph);
            streamWriter.Write(contentWriter);
        }

        public virtual string Serialize<T>(T instance)
        {
            using var contentWriter = new StringWriter();
            Serialize(new JsonTextWriter(contentWriter), instance);
            return contentWriter.ToString();
        }

        public virtual T Deserialize<T>(string json)
        {
            return Deserialize<T>(new JsonTextReader(new StringReader(json)));
        }

        public virtual T Deserialize<T>(Stream input)
        {
            using var streamReader = new StreamReader(input, Encoding.UTF8);
            return Deserialize<T>(streamReader.ReadToEnd());
        }

        protected virtual void Serialize(JsonWriter writer, object graph)
        {
            using (writer)
            {
                GetSerializer(graph.GetType()).Serialize(writer, graph);
            }
        }

        protected virtual T Deserialize<T>(JsonReader reader)
        {
            Type type = typeof(T);

            using (reader)
            {
                return (T)GetSerializer(type).Deserialize(reader, type);
            }
        }

        protected virtual JsonSerializer GetSerializer(Type typeToSerialize)
        {
            return _knownTypes.Contains(typeToSerialize) ? _untypedSerializer : _typedSerializer;
        }
    }
}