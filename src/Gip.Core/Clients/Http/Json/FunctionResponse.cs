using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Gip.Core.Clients.Http.Json
{

    public class FunctionResponse
    {

        [JsonPropertyName("o")]
        public required ImmutableArray<FunctionOutputResponse> Outputs { get; set; }

    }

}
