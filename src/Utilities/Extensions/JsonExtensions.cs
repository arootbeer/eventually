using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace Eventually.Utilities.Extensions
{
    public static class Type<T>
    {
        public static T From(object dto) => FromJson(dto.ToJson(false));

        public static T FromJson(string json) => JsonConvert.DeserializeObject<T>(
            json, 
            JsonExtensions.jsonSerializerSettings
        );
    }
    
    public static class JsonExtensions
    {
        internal static readonly JsonSerializerSettings jsonSerializerSettings;

        static JsonExtensions()
        {
            jsonSerializerSettings = new JsonSerializerSettings
            {
                Error = (_, ev) => ev.ErrorContext.Handled = true,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None
            };
            jsonSerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        }
        public static object HydrateFrom(this Type destinationType, IDictionary<string, object> data)
        {
            return destinationType.HydrateFrom(data.ToJson(false));
        }
        
        public static object HydrateFrom(this Type destinationType, string json)
        {
            var result = JsonConvert.DeserializeObject(
                json,
                destinationType,
                jsonSerializerSettings
            );
            
            return result;
        }
        
        public static string ToJson<T>(this T input, bool indent = true)
        {
            var json = JsonConvert.SerializeObject(
                input,
                indent ? Formatting.Indented : Formatting.None,
                jsonSerializerSettings
            );

            return json;
        }
    }
}
