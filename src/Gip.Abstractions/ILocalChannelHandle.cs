using System;

namespace Gip.Abstractions
{

    public interface ILocalChannelHandle : IChannelHandle
    {

        /// <summary>
        /// Gets the ID of the channel.
        /// </summary>
        Guid Id { get; }

    }

}
