using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

        public void AppendRange(ReadOnlyMemory<T> items)
        {
            _writer.Write(new SequenceAppendManySignal<T>(items));
        }

        public void AppendRange(ImmutableArray<T> items)
        {
            _writer.Write(new SequenceAppendManySignal<T>(items.AsMemory()));
        }

        public void AppendRange(IReadOnlyList<T> items)
        {
            _writer.Write(new SequenceAppendManySignal<T>(items.ToArray().AsMemory()));
        }

        public void AppendRange(IImmutableList<T> items)
        {
            _writer.Write(new SequenceAppendManySignal<T>(items.ToArray().AsMemory()));
        }

        public void AppendRange(params T[] items)
        {
            _writer.Write(new SequenceAppendManySignal<T>(items.AsMemory()));
        }

        public void AppendRange(params ReadOnlySpan<T> items)
        {
            _writer.Write(new SequenceAppendManySignal<T>(items.ToArray().AsMemory()));
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
