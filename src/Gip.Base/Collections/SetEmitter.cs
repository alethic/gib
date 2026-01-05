using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Gip.Abstractions;

namespace Gip.Base.Collections
{

    public readonly struct SetEmitter<T> : IDisposable
    {

        readonly IChannelWriter<SetSignal<T>> _writer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="writer"></param>
        public SetEmitter(IChannelWriter<SetSignal<T>> writer)
        {
            _writer = writer;
        }

        public void Add(T item)
        {
            _writer.Write(new SetAddSignal<T>(item));
        }

        public void AddRange(ImmutableArray<T> items)
        {
            _writer.Write(new SetAddManySignal<T>(items));
        }

        public void AddRange(IReadOnlyCollection<T> items)
        {
            _writer.Write(new SetAddManySignal<T>([.. items]));
        }

        public void AddRange(ImmutableHashSet<T> items)
        {
            _writer.Write(new SetAddManySignal<T>([.. items]));
        }

        public void AddRange(params T[] items)
        {
            _writer.Write(new SetAddManySignal<T>([.. items]));
        }

        public void AddRange(params ReadOnlySpan<T> items)
        {
            _writer.Write(new SetAddManySignal<T>(items.ToImmutableArray()));
        }

        public void Remove(T item)
        {
            _writer.Write(new SetRemoveSignal<T>(item));
        }

        public void RemoveRange(ImmutableArray<T> items)
        {
            _writer.Write(new SetRemoveManySignal<T>(items));
        }

        public void RemoveRange(IReadOnlyCollection<T> items)
        {
            _writer.Write(new SetRemoveManySignal<T>([.. items]));
        }

        public void RemoveRange(ImmutableHashSet<T> items)
        {
            _writer.Write(new SetRemoveManySignal<T>([.. items]));
        }

        public void RemoveRange(params T[] items)
        {
            _writer.Write(new SetRemoveManySignal<T>([.. items]));
        }

        public void RemoveRange(params ReadOnlySpan<T> items)
        {
            _writer.Write(new SetRemoveManySignal<T>(items.ToImmutableArray()));
        }

        public void Clear()
        {
            _writer.Reset();
            _writer.Write(new SetClearSignal<T>());
        }

        public void Freeze()
        {
            _writer.Write(new SetFreezeSignal<T>());
        }

        public void Resume()
        {
            _writer.Write(new SetResumeSignal<T>());
        }

        public void Dispose()
        {
            _writer.Dispose();
        }

    }

}
