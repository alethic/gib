using System;
using System.Threading;
using System.Threading.Tasks;

using Gib.Hosting.Abstractions;

namespace Gib.Hosting.Fetchers.File
{

    public class FileFetcher : IFetcher
    {

        public ValueTask<Uri?> FetchAsync(Uri fetchUri, CancellationToken cancellationToken)
        {
            if (fetchUri.Scheme == "file")
                return new(fetchUri);

            return default;
        }

    }

}
