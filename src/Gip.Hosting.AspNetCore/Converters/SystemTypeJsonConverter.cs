using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gip.Hosting.AspNetCore.Converters
{

    public class SystemTypeJsonConverter : JsonConverter<Type>
    {

        public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.FullName, options);
        }

    }

}
