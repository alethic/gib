namespace Gip.Abstractions
{

    public interface IWritableChannelHandle : IChannelHandle
    {

        /// <summary>
        /// Opens a channel for writing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IChannelWriter<T> Writer<T>();

        /// <summary>
        /// Binds the writable channel to the specified readable channel.
        /// </summary>
        /// <param name="source"></param>
        void Bind(IReadableChannelHandle source);

    }

}
