using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using Gip.Abstractions.Clients;
using Gip.Core.Clients.Http.Json;

namespace Gip.Core.Clients.Http
{

    public sealed class HttpCallClient : ICallClient
    {

        readonly HttpClient _http;
        readonly HttpResponseMessage _response;
        readonly StreamReader _reader;
        readonly CallResponse _callResponse;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="http"></param>
        /// <param name="response"></param>
        /// <param name="reader"></param>
        /// <param name="callResponse"></param>
        public HttpCallClient(HttpClient http, HttpResponseMessage response, StreamReader reader, CallResponse callResponse)
        {
            _http = http;
            _response = response;
            _reader = reader;
            _callResponse = callResponse;
        }

        /// <inheritdoc />
        public IChannelClient<T> GetOutputChannel<T>(int index)
        {
            return new HttpChannelClient<T>(_http, _callResponse.Outputs[index]);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _reader.Dispose();
            _response.Dispose();
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            _reader.Dispose();
            _response.Dispose();
            return default;
        }
    }

}