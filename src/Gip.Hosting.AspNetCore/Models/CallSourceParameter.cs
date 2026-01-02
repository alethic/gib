using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Gip.Hosting.AspNetCore.Models
{

    public class CallSourceParameter
    {

        [JsonPropertyName("r")]
        public Uri? Remote { get; set; }

        [JsonPropertyName("s")]
        public JsonNode[]? Signals { get; set; }

    }

}
