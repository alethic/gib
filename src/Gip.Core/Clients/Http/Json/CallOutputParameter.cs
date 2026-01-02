using System;
using System.Text.Json.Serialization;

namespace Gip.Core.Clients.Http.Json
{

    /// <summary>
    /// JSON-serializable call output parameter.
    /// </summary>
    public class CallOutputParameter
    {

        [JsonPropertyName("r")]
        public required Uri Uri { get; set; }

    }

}
