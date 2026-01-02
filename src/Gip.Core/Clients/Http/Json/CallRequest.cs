using System.Text.Json.Serialization;

namespace Gip.Core.Clients.Http.Json
{

    /// <summary>
    /// JSON-serializable call request.
    /// </summary>
    public class CallRequest
    {

        [JsonPropertyName("s")]
        public required CallSourceParameter[] Sources { get; set; }

    }

}
