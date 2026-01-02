using System.Collections.Immutable;

namespace Gib.Reflection
{

    public record class ElementType(string AssemblyName, string TypeName, ImmutableArray<ElementProperty> Properties)
    {



    }

}
