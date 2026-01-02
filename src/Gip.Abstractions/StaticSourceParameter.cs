using System.Collections.Immutable;

namespace Gip.Abstractions
{

    public record class StaticSourceParameter(ImmutableArray<object?> Signals) :
        SourceParameter
    {



    }

}
