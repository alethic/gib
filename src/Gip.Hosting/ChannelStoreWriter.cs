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
        readonly Action? _release;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="release"></param>
        public ChannelStoreWriter(IChannelStore<T> store, Action? release)
        {
            _store = store;
            _release = release;
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
            GC.SuppressFinalize(this);

            if (_release is not null)
                _release();
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
