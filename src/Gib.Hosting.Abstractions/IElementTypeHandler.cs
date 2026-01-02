
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gib.Hosting.Abstractions
{

    /// <summary>
    /// An <see cref="IElementTypeHandler"/> provides support for one or more Element Types by checking its Element Type URI.
    /// </summary>
    public interface IElementTypeHandler
    {

        ValueTask<ElementTypeDescriptor?> GetElementTypeAsync(Uri elementTypeUri, CancellationToken cancellationToken);

    }

}
