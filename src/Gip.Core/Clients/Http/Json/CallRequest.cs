using System;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Gip.Core.Clients.Http.Json
{

    /// <summary>
    /// JSON-serializable call request.
    /// </summary>
    public class CallRequest
    {

        [JsonPropertyName("s")]
        public required ImmutableArray<Uri> Sources { get; set; }

    }

}
