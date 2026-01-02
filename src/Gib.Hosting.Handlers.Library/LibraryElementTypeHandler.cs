
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Gib.Hosting;
using Gib.Hosting.Abstractions;
using Gib.Hosting.Handlers.Library;

namespace Gib.Containers.Library
{

    /// <summary>
    /// Provides functionality for handling library element types. Library element types point to a directory structure with a dependecy context.
    /// </summary>
    public class LibraryElementTypeHandler : IElementTypeHandler
    {

        readonly IEnumerable<IFetcher> _fetchers;
        readonly LibraryHost _libraryHost;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="fetchers"></param>
        /// <param name="libraryHost"></param>
        public LibraryElementTypeHandler(IEnumerable<IFetcher> fetchers, LibraryHost libraryHost)
        {
            _fetchers = fetchers;
            _libraryHost = libraryHost;
        }

        /// <inheritdoc />
        public ValueTask<ElementTypeDescriptor?> GetElementTypeAsync(Uri elementTypeUri, CancellationToken cancellationToken)
        {

            if (elementTypeUri.Scheme == "lib")
                return GetLibraryElementTypeAsync(new Uri(elementTypeUri.AbsolutePath), cancellationToken);

            return default;
        }

        /// <summary>
        /// Looks up the element type for the given library URI.
        /// </summary>
        /// <param name="elementTypeUri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async ValueTask<ElementTypeDescriptor?> GetLibraryElementTypeAsync(Uri elementTypeUri, CancellationToken cancellationToken)
        {
            foreach (var fetcher in _fetchers)
                if (await fetcher.FetchAsync(elementTypeUri, cancellationToken) is Uri localUri)
                    if (localUri.Scheme == "file")
                        return await _libraryHost.GetElementTypeAsync(localUri, cancellationToken);

            return null;
        }

    }

}
