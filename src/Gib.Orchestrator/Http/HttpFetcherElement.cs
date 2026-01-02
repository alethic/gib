using System;
using System.Threading;
using System.Threading.Tasks;

using Gib.Core.Elements;

namespace Gib.Orchestrator.Http
{

    [Element]
    class HttpFetcherElement : ElementBase, IElementWithProxy<IHttpFetcher>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public HttpFetcherElement(IElementContext context) : 
            base(context)
        {

        }

        public required Uri HttpUri { get; set; }

        public string? LocalPath { get; set; }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // TODO download from HTTP URL and place into cache, then set LocalPath
        }

    }

}
