using System.Text.Json.Serialization;

namespace Gip.Hosting.AspNetCore.Models
{

    public class CallRequest
    {

        [JsonPropertyName("s")]
        public required CallSourceParameter[] Sources { get; set; }

    }

}
