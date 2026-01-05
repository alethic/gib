using System;
using System.Text.Json.Serialization;

namespace Gip.Core.Clients.Http.Json
{

    public class FunctionOutputResponse
    {

        [JsonPropertyName("u")]
        public Uri? Uri { get; }

        [JsonPropertyName("s")]
        public string? Name { get; }

    }

}