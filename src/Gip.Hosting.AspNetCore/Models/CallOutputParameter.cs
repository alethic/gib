using System;
using System.Text.Json.Serialization;

namespace Gip.Hosting.AspNetCore.Models
{

    public class CallOutputParameter
    {

        [JsonPropertyName("r")]
        public Uri? Uri { get; set; }

    }

}
