using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Gip.Core.Clients.Http.Json
{

    /// <summary>
    /// JSON-serializable call source parameter.
    /// </summary>
    public class CallSourceParameter
    {

        [JsonPropertyName("r")]
        public Uri? Remote { get; set; }

        [JsonPropertyName("s")]
        public JsonNode[]? Static { get; set; }

    }

}
