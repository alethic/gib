using System;
using System.Threading;
using System.Threading.Tasks;

using Gib.Hosting.Abstractions;

namespace Gib.Hosting.Fetchers.Git
{

    /// <summary>
    /// Implements a <see cref="IFetcher"/> that supports Git repositories.
    /// </summary>
    public class GitFetcher : IFetcher
    {

        public ValueTask<Uri?> FetchAsync(Uri fetchUri, CancellationToken cancellationToken)
        {
            if (fetchUri.Scheme == "git")
                return default; // TODO implement

            return default;
        }

    }

}
