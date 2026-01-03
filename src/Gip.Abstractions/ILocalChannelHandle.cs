using System;

namespace Gip.Abstractions
{

    public interface ILocalChannelHandle : IChannelHandle, IReadableChannelHandle, IWritableChannelHandle
    {

        /// <summary>
        /// Gets the ID of the channel.
        /// </summary>
        Guid Id { get; }

    }

}
