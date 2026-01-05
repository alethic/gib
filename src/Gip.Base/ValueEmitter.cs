using System;

using Gip.Abstractions;

namespace Gip.Base
{

    public readonly struct ValueEmitter<T> : IDisposable
    {

        readonly IChannelWriter<ValueSignal<T>> _writer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="writer"></param>
        public ValueEmitter(IChannelWriter<ValueSignal<T>> writer)
        {
            _writer = writer;
        }

        public void Set(T item)
        {
            _writer.Reset();
            _writer.Write(new SetValueSignal<T>(item));
        }

        public void Dispose()
        {
            _writer.Dispose();
        }

    }

}
