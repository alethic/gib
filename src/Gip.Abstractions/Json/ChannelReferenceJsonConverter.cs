using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gip.Abstractions.Json
{

    class ChannelReferenceJsonConverter : JsonConverter<ChannelReference>
    {

        public override ChannelReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new ChannelReference(JsonSerializer.Deserialize<Uri>(ref reader, options) ?? throw new InvalidOperationException());
        }

        public override void Write(Utf8JsonWriter writer, ChannelReference value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.Uri, options);
        }

    }

}
