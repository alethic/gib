using System.Collections.Immutable;
using System.Text.Json.Serialization;

using ProtoBuf.Meta;

namespace Gip.Abstractions
{

    /// <summary>
    /// Describes the schema of a function.
    /// </summary>
    [JsonConverter(typeof(FunctionSchemaJsonConverter))]
    public record class FunctionSchema(TypeModel TypeModel, ImmutableArray<ChannelSchema> Sources, ImmutableArray<ChannelSchema> Outputs)
    {

        public static FunctionSchemaBuilder CreateBuilder()
        {
            return new FunctionSchemaBuilder();
        }

    }

}
