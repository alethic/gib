using System;

using Gip.Abstractions;

namespace Gip.Hosting
{

    /// <summary>
    /// <see cref="IChannelWriter{T}"/> implementation that writes to a <see cref="IChannelStore{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class ChannelStoreWriter<T> : IChannelWriter<T>
    {

        readonly IChannelStore<T> _store;
        readonly Action? _dispose;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="dispose"></param>
        public ChannelStoreWriter(IChannelStore<T> store, Action? dispose)
        {
            _store = store;
            _dispose = dispose;
        }

        /// <inheritdoc />
        public void Write(T signal)
        {
            _store.Store(signal);
        }

        /// <inheritdoc />
        public void Reset()
        {
            _store.Reset();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _dispose?.Invoke();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes the instance.
        /// </summary>
        ~ChannelStoreWriter()
        {
            Dispose();
        }

    }

}
