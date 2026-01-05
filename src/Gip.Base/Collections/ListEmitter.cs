using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Gip.Abstractions;

namespace Gip.Base.Collections
{

    public readonly struct ListEmitter<T> : IDisposable
    {

        readonly IChannelWriter<ListSignal<T>> _writer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="writer"></param>
        public ListEmitter(IChannelWriter<ListSignal<T>> writer)
        {
            _writer = writer;
        }

        public void Set(Index index, T item)
        {
            _writer.Write(new ListSetSignal<T>(index, item));
        }

        public void SetMany(Index index, ImmutableArray<T> items)
        {
            _writer.Write(new ListSetManySignal<T>(index, items));
        }

        public void SetMany(Index index, IReadOnlyList<T> items)
        {
            _writer.Write(new ListSetManySignal<T>(index, [.. items]));
        }

        public void SetMany(Index index, IImmutableList<T> items)
        {
            _writer.Write(new ListSetManySignal<T>(index, [.. items]));
        }

        public void SetMany(Index index, params T[] items)
        {
            _writer.Write(new ListSetManySignal<T>(index, [.. items]));
        }

        public void SetMany(Index index, params ReadOnlySpan<T> items)
        {
            _writer.Write(new ListSetManySignal<T>(index, items.ToImmutableArray()));
        }

        public void Insert(Index index, T item)
        {
            _writer.Write(new ListInsertSignal<T>(index, item));
        }

        public void InsertRange(Index index, ImmutableArray<T> items)
        {
            _writer.Write(new ListInsertManySignal<T>(index, items));
        }

        public void InsertRange(Index index, IReadOnlyList<T> items)
        {
            _writer.Write(new ListInsertManySignal<T>(index, [.. items]));
        }

        public void InsertRange(Index index, IImmutableList<T> items)
        {
            _writer.Write(new ListInsertManySignal<T>(index, [.. items]));
        }

        public void InsertRange(Index index, params T[] items)
        {
            _writer.Write(new ListInsertManySignal<T>(index, [.. items]));
        }

        public void InsertRange(Index index, params ReadOnlySpan<T> items)
        {
            _writer.Write(new ListInsertManySignal<T>(index, items.ToImmutableArray()));
        }

        public void Remove(Index index)
        {
            _writer.Write(new ListRemoveSignal<T>(index));
        }

        public void RemoveRange(Range range)
        {
            _writer.Write(new ListRemoveManySignal<T>(range));
        }

        public void Clear()
        {
            _writer.Reset();
            _writer.Write(new ListClearSignal<T>());
        }

        public void Freeze()
        {
            _writer.Write(new ListFreezeSignal<T>());
        }

        public void Resume()
        {
            _writer.Write(new ListResumeSignal<T>());
        }

        public void Dispose()
        {
            _writer.Dispose();
        }

    }

}
