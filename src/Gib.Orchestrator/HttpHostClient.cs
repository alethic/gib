using System;

namespace Gib.Orchestrator
{

    public class HttpHostClient : IHostClient
    {

        readonly Uri _baseUri;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="baseUri"></param>
        public HttpHostClient(Uri baseUri)
        {
            _baseUri = baseUri;
        }

    }

}
