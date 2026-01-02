using System;

namespace Gip.Abstractions
{

    /// <summary>
    /// Output from a call that specifies a channel output parameter.
    /// </summary>
    public record class OutputParameter(Guid Id, ChannelSchema Schema)
    {



    }

}
