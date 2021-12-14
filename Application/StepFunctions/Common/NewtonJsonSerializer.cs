using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using JsonSerializer = Amazon.Lambda.Serialization.Json.JsonSerializer;

namespace Common
{
    public class NewtonJsonSerializer : JsonSerializer
    {
        public NewtonJsonSerializer()
            : base(
                // Step 1: Customize the serializer settings
                CustomizeSerializerSettings,
                // Step 2: Use Pascal Case
                new CamelCaseNamingStrategy()
            )
        {
        }

        private static void CustomizeSerializerSettings(JsonSerializerSettings settings)
        {
            settings.Converters = new List<JsonConverter>
            {
                new StringEnumConverter
                {
                    CamelCaseText = false
                }
            };
            settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.TypeNameHandling = TypeNameHandling.None;
        }
    }
}