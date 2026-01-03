namespace Gip.Abstractions
{

    public interface IWritableChannelHandle : IChannelHandle
    {

        /// <summary>
        /// Opens a channel for writing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IChannelWriter<T> OpenWrite<T>();

    }

}
