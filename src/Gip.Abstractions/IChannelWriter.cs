using System;

namespace Gip.Abstractions
{

    /// <summary>
    /// Provides an interface for writing to an output channel.
    /// </summary>
    public interface IChannelWriter : IDisposable
    {

    }
    
    /// <summary>
    /// Provides an interface for writing to an output channel.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IChannelWriter<T> : IChannelWriter
    {

        /// <summary>
        /// Writes the specified signal to the channel.
        /// </summary>
        /// <param name="signal"></param>
        void Write(T signal);

        /// <summary>
        /// Resets the channel.
        /// </summary>
        void Reset();

    }

}
