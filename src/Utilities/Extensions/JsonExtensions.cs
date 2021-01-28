using System;
using Eventually.Interfaces.Common.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace Eventually.Utilities.Extensions
{
    public static class JsonExtensions
    {
        public static string ToJson<T>(this T input, bool indent = true)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new InterfaceContractResolver(),
                Error = (se, ev) => ev.ErrorContext.Handled = true,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None
            };
            jsonSerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            
            var json = JsonConvert.SerializeObject(
                input,
                indent ? Formatting.Indented : Formatting.None,
                jsonSerializerSettings
            );

            return json;
        }

        private class InterfaceContractResolver : DefaultContractResolver
        {
            protected override JsonContract CreateContract(Type objectType)
            {
                var serializationType = objectType.GetMostDerivedImplementationType<IMessage>() ?? objectType;
                var jsonContract = base.CreateContract(serializationType);
                return jsonContract;
            }
        }
    }
}
