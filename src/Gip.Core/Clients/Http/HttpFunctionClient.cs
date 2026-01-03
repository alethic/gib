using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Abstractions.Clients;
using Gip.Core.Clients.Http.Json;

namespace Gip.Core.Clients.Http
{

    public sealed class HttpFunctionClient : IFunctionClient
    {

        readonly HttpClient _http;
        readonly Uri _uri;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="uri"></param>
        public HttpFunctionClient(HttpClient http, Uri uri)
        {
            _http = http;
            _uri = uri;
        }

        /// <inheritdoc />
        public async ValueTask<ICallClient> CallAsync(ImmutableArray<Uri> sources, CancellationToken cancellationToken)
        {
            var response = await _http.PutAsJsonAsync(_uri, new CallRequest() { Sources = sources }, cancellationToken);
            response.EnsureSuccessStatusCode();

            if (response.Content.Headers.ContentType?.MediaType != "application/jsonl")
                throw new InvalidOperationException("Only application/jsonl is presently supported.");

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: false);
            var firstL = await reader.ReadLineAsync(cancellationToken);
            if (firstL is null)
                throw new InvalidOperationException("First line must be JSON.");

            var callResponse = JsonSerializer.Deserialize<CallResponse>(firstL);
            if (callResponse is null)
                throw new InvalidOperationException("First line must be a call response.");

            return new HttpCallClient(_http, response, reader, callResponse);
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