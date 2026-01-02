using System.Text.Json.Serialization;

namespace Gip.Hosting.AspNetCore.Models
{

    public class CallResult
    {

        [JsonPropertyName("o")]
        public required CallOutputParameter[]? Outputs { get; set; }

    }

}
