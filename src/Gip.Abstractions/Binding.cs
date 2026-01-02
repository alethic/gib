namespace Gip.Abstractions
{

    /// <summary>
    /// Describes a reference to a channel made available to a function call.
    /// </summary>
    public abstract class Binding
    {

        /// <summary>
        /// Gets the schema of the signals available over the channel.
        /// </summary>
        public abstract ChannelSchema Schema { get; }

    }

}
