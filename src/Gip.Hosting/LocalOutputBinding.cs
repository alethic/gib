using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;

namespace Gip.Hosting
{

    /// <summary>
    /// Implementation of <see cref="OutputBinding"/> which is associated with a local <see cref="IChannelStore"/> in the current host.
    /// </summary>
    class LocalOutputBinding : OutputBinding
    {

        class ChannelWriter<T> : IChannelWriter<T>
            where T : notnull
        {

            readonly IChannelStore<T> _store;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="store"></param>
            public ChannelWriter(IChannelStore<T> store)
            {
                _store = store;
            }
            public void Write(T signal)
            {
                _store.Store(signal);
            }

            public void Complete()
            {
                _store.Complete();
            }

            public void Reset()
            {
                _store.Reset();
            }

            public void Dispose()
            {
                Complete();
            }

            public ValueTask DisposeAsync()
            {
                Complete();
                return default;
            }

        }

        readonly ChannelSchema _schema;
        readonly LocalChannel _channel;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="channel"></param>
        public LocalOutputBinding(ChannelSchema schema, LocalChannel channel)
        {
            _schema = schema;
            _channel = channel;
        }

        /// <inheritdoc />
        public override ChannelSchema Schema => _schema;

        /// <inheritdoc />
        public override ValueTask<IChannelWriter<T>> OpenAsync<T>(CancellationToken cancellationToken)
        {
            return new ValueTask<IChannelWriter<T>>(new ChannelWriter<T>((IChannelStore<T>)_channel.Store));
        }

    }

}
