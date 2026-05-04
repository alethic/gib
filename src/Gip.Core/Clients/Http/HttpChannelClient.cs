using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions.Clients;

namespace Gip.Core.Clients.Http
{

    public sealed class HttpChannelClient<TSignal> : IChannelClient<TSignal>
    {

        readonly HttpClient _http;
        readonly Uri _uri;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="http"></param>
        /// <param name="uri"></param>
        /// <param name="clients"></param>
        public HttpChannelClient(HttpClient http, Uri uri)
        {
            _http = http;
            _uri = uri;
        }

        /// <inheritdoc />
        public async IAsyncEnumerator<ChannelEvent<TSignal>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            await foreach (var evnt in _http.GetFromJsonAsAsyncEnumerable<ChannelEvent<TSignal>>(_uri, cancellationToken))
                if (evnt is not null)
                    yield return evnt;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _http.Dispose();
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            _http.Dispose();
            return default;
        }

    }

}