using System;
using System.Text.Json.Serialization;

using Gip.Abstractions.Json;

namespace Gip.Abstractions
{

    [JsonConverter(typeof(FunctionReferenceJsonConverter))]
    public readonly record struct FunctionReference(Uri Uri)
    {

    }

}
