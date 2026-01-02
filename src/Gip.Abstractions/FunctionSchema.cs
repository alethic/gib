using System.Collections.Immutable;

namespace Gip.Abstractions
{

    /// <summary>
    /// Describes the schema of a function.
    /// </summary>
    public record class FunctionSchema(ImmutableArray<ChannelSchema> Sources, ImmutableArray<ChannelSchema> Outputs);

}
