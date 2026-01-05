using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Gip.Abstractions;

namespace Gip.Base.Collections
{

    public readonly struct SequenceEmitter<T> : IDisposable
    {

        readonly IChannelWriter<SequenceSignal<T>> _writer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="writer"></param>
        public SequenceEmitter(IChannelWriter<SequenceSignal<T>> writer)
        {
            _writer = writer;
        }

        public void Append(T item)
        {
            _writer.Write(new SequenceAppendSignal<T>(item));
        }

        public void AppendMany(ImmutableArray<T> items)
        {
            _writer.Write(new SequenceAppendManySignal<T>(items));
        }

        public void AppendMany(IReadOnlyList<T> items)
        {
            _writer.Write(new SequenceAppendManySignal<T>([.. items]));
        }

        public void AppendMany(IImmutableList<T> items)
        {
            _writer.Write(new SequenceAppendManySignal<T>([.. items]));
        }

        public void AppendMany(params T[] items)
        {
            _writer.Write(new SequenceAppendManySignal<T>([.. items]));
        }

        public void AppendMany(params ReadOnlySpan<T> items)
        {
            _writer.Write(new SequenceAppendManySignal<T>(items.ToImmutableArray()));
        }

        public void Clear()
        {
            _writer.Reset();
            _writer.Write(new SequenceClearSignal<T>());
        }

        public void Freeze()
        {
            _writer.Write(new SequenceFreezeSignal<T>());
        }

        public void Resume()
        {
            _writer.Write(new SequenceResumeSignal<T>());
        }

        public void Dispose()
        {
            _writer.Dispose();
        }

    }

}
