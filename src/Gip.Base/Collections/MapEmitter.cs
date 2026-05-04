using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Gip.Abstractions;

namespace Gip.Base.Collections
{

    public readonly struct MapEmitter<TKey, TValue> : IDisposable
        where TKey : notnull
    {

        readonly IChannelWriter<MapSignal<TKey, TValue>> _writer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="writer"></param>
        public MapEmitter(IChannelWriter<MapSignal<TKey, TValue>> writer)
        {
            _writer = writer;
        }

        public void Add(TKey key, TValue value)
        {
            _writer.Write(new MapPutSignal<TKey, TValue>(key, value));
        }

        public void AddRange(IReadOnlyDictionary<TKey, TValue> items)
        {
            _writer.Write(new MapPutManySignal<TKey, TValue>([.. items]));
        }

        public void AddRange(ImmutableDictionary<TKey, TValue> items)
        {
            _writer.Write(new MapPutManySignal<TKey, TValue>(items));
        }

        public void Remove(TKey key)
        {
            _writer.Write(new MapRemoveSignal<TKey, TValue>(key));
        }

        public void RemoveRange(ImmutableArray<TKey> keys)
        {
            _writer.Write(new MapRemoveManySignal<TKey, TValue>(keys));
        }

        public void RemoveRange(IReadOnlyCollection<TKey> items)
        {
            _writer.Write(new MapRemoveManySignal<TKey, TValue>([.. items]));
        }

        public void RemoveRange(ImmutableHashSet<TKey> items)
        {
            _writer.Write(new MapRemoveManySignal<TKey, TValue>([.. items]));
        }

        public void RemoveRange(params TKey[] items)
        {
            _writer.Write(new MapRemoveManySignal<TKey, TValue>([.. items]));
        }

        public void RemoveRange(params ReadOnlySpan<TKey> items)
        {
            _writer.Write(new MapRemoveManySignal<TKey, TValue>(items.ToImmutableArray()));
        }

        public void Clear()
        {
            _writer.Reset();
            _writer.Write(new MapClearSignal<TKey, TValue>());
        }

        public void Freeze()
        {
            _writer.Write(new MapFreezeSignal<TKey, TValue>());
        }

        public void Resume()
        {
            _writer.Write(new MapResumeSignal<TKey, TValue>());
        }

        public void Dispose()
        {
            _writer.Dispose();
        }

    }

}
