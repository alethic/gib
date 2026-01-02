using System;

namespace Gip.Abstractions
{

    /// <summary>
    /// Describes the schema of a channel.
    /// </summary>
    /// <param name="Signal"></param>
    public record class ChannelSchema(Type Signal);

}
