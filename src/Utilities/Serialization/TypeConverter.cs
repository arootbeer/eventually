using Eventually.Utilities.Extensions;
using Newtonsoft.Json;

namespace Eventually.Utilities.Serialization
{
    public static class TypeConverter<T>
    {
        public static T From(object dto) => FromJson(dto.ToJson(false));

        public static T FromJson(string json) => JsonConvert.DeserializeObject<T>(
            json, 
            JsonExtensions.jsonSerializerSettings
        );
    }
}