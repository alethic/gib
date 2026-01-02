using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions.Clients;

namespace Gip.Core.Clients.Http
{

    public sealed class HttpChannelClient<T> : IChannelClient<T>
    {

        readonly HttpClient _http;
        readonly Uri _uri;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="uri"></param>
        public HttpChannelClient(HttpClient http, Uri uri)
        {
            _http = http;
            _uri = uri;
        }

        /// <inheritdoc />
        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            await foreach (var i in _http.GetFromJsonAsAsyncEnumerable<T>(_uri, cancellationToken))
                yield return i ?? throw new InvalidOperationException();
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