using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

using Gip.Abstractions.Clients;

namespace Gip.Core.Clients.Http
{

    /// <summary>
    /// Implementation of <see cref="IClientProtocol"/> for http and https endpoints.
    /// </summary>
    public sealed class HttpClientProtocol : IClientProtocol
    {

        readonly IHttpClientFactory _http;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="http"></param>
        public HttpClientProtocol(IHttpClientFactory http)
        {
            _http = http;
        }

        /// <inheritdoc />
        public bool TryGetFunction(Uri uri, [NotNullWhen(true)] out IFunctionClient? client)
        {
            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                client = null;
                return false;
            }

            client = new HttpFunctionClient(_http.CreateClient(), uri);
            return true;
        }

        /// <inheritdoc />
        public bool TryGetChannel<T>(Uri uri, [NotNullWhen(true)] out IChannelClient<T>? client)
        {
            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                client = null;
                return false;
            }

            client = new HttpChannelClient<T>(_http.CreateClient(), uri);
            return true;
        }

    }

}
