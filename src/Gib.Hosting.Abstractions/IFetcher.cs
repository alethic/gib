using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gib.Hosting.Abstractions
{

    /// <summary>
    /// Provides support for a single 'delivery protocol'. That is, 'https', 'git', 'file', etc. Resolves that URI into a local directory path.
    /// </summary>
    public interface IFetcher
    {

        /// <summary>
        /// Resolves the given URI into a local 'file' reference.
        /// </summary>
        /// <param name="fetchUri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<Uri?> FetchAsync(Uri fetchUri, CancellationToken cancellationToken);

    }

}
