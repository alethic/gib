namespace Gip.Abstractions
{

    public interface IChannelHandle
    {

        /// <summary>
        /// Gets the schema of the channel.
        /// </summary>
        ChannelSchema Schema { get; }

    }

}
