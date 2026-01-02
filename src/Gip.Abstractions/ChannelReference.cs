using System;
using System.Text.Json.Serialization;

using Gip.Abstractions.Json;

namespace Gip.Abstractions
{

    [JsonConverter(typeof(ChannelReferenceJsonConverter))]
    public readonly record struct ChannelReference(Uri Uri)
    {

    }

}
