using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Gip.Abstractions;

using ProtoBuf.Meta;

namespace Gip.Abstractions
{

    class FunctionSchemaJsonConverter : JsonConverter<FunctionSchema>
    {

        public override FunctionSchema? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, FunctionSchema value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("s");
            writer.WriteStartArray();
            foreach (var source in value.Sources)
                WriteChannel(writer, value.TypeModel, source, options);
            writer.WriteEndArray();

            writer.WritePropertyName("o");
            writer.WriteStartArray();
            foreach (var output in value.Outputs)
                WriteChannel(writer, value.TypeModel, output, options);
            writer.WriteEndArray();

            writer.WritePropertyName("p");
            writer.WriteStringValue(value.TypeModel.GetSchema(null, ProtoSyntax.Proto3));

            writer.WriteEndObject();
        }

        void WriteChannel(Utf8JsonWriter writer, TypeModel model, ChannelSchema value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("n");
            writer.WriteStringValue(value.Signal.GetSchemaTypeName());
            writer.WritePropertyName("p");
            writer.WriteStringValue(model.GetSchema(value.Signal.Type, ProtoSyntax.Proto3));
            writer.WriteEndObject();
        }

    }

}
