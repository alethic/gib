using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gip.Abstractions.Json
{

    class FunctionReferenceJsonConverter : JsonConverter<FunctionReference>
    {

        public override FunctionReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new FunctionReference(JsonSerializer.Deserialize<Uri>(ref reader, options) ?? throw new InvalidOperationException());
        }

        public override void Write(Utf8JsonWriter writer, FunctionReference value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.Uri, options);
        }

    }

}
