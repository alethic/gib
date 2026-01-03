using System;

namespace Gip.Abstractions
{

    public interface IRemoteChannelHandle : IReadableChannelHandle
    {

        /// <summary>
        /// Gets the URI of the remote channel.
        /// </summary>
        Uri Uri { get; }

    }

}
