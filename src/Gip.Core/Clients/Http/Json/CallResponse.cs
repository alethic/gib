using System.Text.Json.Serialization;

namespace Gip.Core.Clients.Http.Json
{

    /// <summary>
    /// JSON-serializable call response.
    /// </summary>
    public class CallResponse
    {

        [JsonPropertyName("o")]
        public required CallOutputParameter[] Outputs { get; set; }

    }

}
