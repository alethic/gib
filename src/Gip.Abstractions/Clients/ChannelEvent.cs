using System;
using System.Text.Json.Serialization;

namespace Gip.Abstractions.Clients
{

    /// <summary>
    /// Describes an event that happened on a channel.
    /// </summary>
    public class ChannelEvent<TSignal>
    {

        public static ChannelEvent<TSignal> FromSignal(TSignal signal)
        {
            return new ChannelEvent<TSignal>() { Signal = signal };
        }

        public static ChannelEvent<TSignal> FromRedirect(Uri uri)
        {
            return new ChannelEvent<TSignal>() { Redirect = uri };
        }

        [JsonPropertyName("s")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TSignal? Signal { get; set; }

        [JsonPropertyName("r")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Uri? Redirect { get; set; }

    }

}
