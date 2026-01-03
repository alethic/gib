using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;

using Nito.AsyncEx;

namespace Gip.Hosting
{

    /// <summary>
    /// Implementation of <see cref="IChannelStore"/> that supports a forward-only linked list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class DefaultChannelStore<T> : IChannelStore<T>
    {

        const int BLOCK_SIZE = 16;

        [InlineArray(BLOCK_SIZE)]
        struct Buffer
        {
            T _element0;
        }

        class Block()
        {
            public int Count = 0;
            public Block? Next = null;
            public Buffer Data = new Buffer();
        }

        readonly AsyncMonitor _monitor = new AsyncMonitor();
        Block _begBlock;
        Block _endBlock;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public DefaultChannelStore()
        {
            _endBlock = new Block();
            _begBlock = _endBlock;
        }

        /// <inheritdoc />
        public void Store(T signal)
        {
            using var l = _monitor.Enter();

            if (_endBlock.Next == _endBlock)
                throw new InvalidOperationException("Channel is already completed.");

            // add a new end block if current end block is full
            if (_endBlock.Count == BLOCK_SIZE)
                _endBlock = _endBlock.Next = new Block();

            // append to last index of end block
            _endBlock.Data[_endBlock.Count++] = signal;

            // notify any readers of new item
            _monitor.Pulse();
        }

        /// <inheritdoc />
        public void Reset()
        {
            using var l = _monitor.Enter();

            if (_endBlock.Next == _endBlock)
                throw new InvalidOperationException("Channel is already completed.");

            // replace both blocks with new empty blocks
            _endBlock = _endBlock.Next = new Block();
            _begBlock = _endBlock;
            _monitor.Pulse();
        }

        /// <inheritdoc />
        public void Complete()
        {
            using var l = _monitor.Enter();

            if (_endBlock.Next == _endBlock)
                return;

            _endBlock.Next = _endBlock;
            _monitor.Pulse();
        }

        /// <inheritdoc />
        public bool IsComplete
        {
            get
            {
                using var l = _monitor.Enter();
                return _endBlock.Next == _endBlock;
            }
        }

        /// <inheritdoc />
        public IAsyncEnumerable<T> OpenAsync(CancellationToken cancellationToken = default)
        {
            return new AsyncEnumerable(this, _monitor, cancellationToken, _begBlock);
        }

        /// <summary>
        /// Provides an enumerable for a channel.
        /// </summary>
        struct AsyncEnumerable : IAsyncEnumerable<T>
        {

            readonly DefaultChannelStore<T> _store;
            readonly AsyncMonitor _monitor;
            readonly CancellationToken _cancellationToken;
            readonly Block _initialBlock;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="store"></param>
            /// <param name="monitor"></param>
            /// <param name="cancellationToken"></param>
            /// <param name="initialBlock"></param>
            public AsyncEnumerable(DefaultChannelStore<T> store, AsyncMonitor monitor, CancellationToken cancellationToken, Block initialBlock)
            {
                _store = store;
                _monitor = monitor;
                _cancellationToken = cancellationToken;
                _initialBlock = initialBlock;
            }

            /// <inheritdoc />
            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new AsyncEnumerator(_store, _monitor, CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken, cancellationToken).Token, _initialBlock);
            }

        }

        /// <summary>
        /// Provides an enumerator for a channel.
        /// </summary>
        class AsyncEnumerator : IAsyncEnumerator<T>
        {

            readonly DefaultChannelStore<T> _store;
            readonly AsyncMonitor _monitor;
            readonly CancellationToken _cancellationToken;

            Block _block;
            int _position;

            /// <summary>
            /// Initializes a new instance starting from the specified block.
            /// </summary>
            /// <param name="store"></param>
            /// <param name="monitor"></param>
            /// <param name="cancellationToken"></param>
            /// <param name="initialBlock"></param>
            public AsyncEnumerator(DefaultChannelStore<T> store, AsyncMonitor monitor, CancellationToken cancellationToken, Block initialBlock)
            {
                _store = store;
                _monitor = monitor;
                _cancellationToken = cancellationToken;
                _block = initialBlock;
                _position = -1;
            }

            /// <inheritdoc />
            public T Current => _position >= 0 ? _block.Data[_position] : throw new InvalidOperationException();

            /// <inheritdoc />
            public async ValueTask<bool> MoveNextAsync()
            {
                using var l = await _monitor.EnterAsync(_cancellationToken);

                while (true)
                {
                    // we can safely move to the next position in the current block
                    if (_position + 1 < _block.Count)
                    {
                        _position = _position + 1;
                        return true;
                    }

                    // block could expand, or a next block could be added, but has not yet
                    if (_block.Count <= BLOCK_SIZE && _block.Next == null)
                    {
                        await _monitor.WaitAsync(_cancellationToken);
                        continue;
                    }

                    // the block is terminal
                    if (_block.Next == _block)
                    {
                        return false;
                    }

                    // move to the next block
                    if (_block.Next != null)
                    {
                        _block = _block.Next;
                        _position = -1;
                        continue;
                    }

                    throw new UnreachableException();
                }
            }

            /// <inheritdoc />
            public ValueTask DisposeAsync()
            {
                return default;
            }

        }


    }

}
