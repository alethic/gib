using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Nito.AsyncEx;

namespace Gip.Core
{

    public class LocalChannel<TSignal> : IChannel<TSignal>, IAsyncEnumerable<TSignal>
    {

        const int BLOCK_SIZE = 16;

        [InlineArray(BLOCK_SIZE)]
        struct Buffer
        {
            TSignal _element0;
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
        public LocalChannel()
        {
            _endBlock = new Block();
            _begBlock = _endBlock;
        }

        /// <inheritdoc />
        public void Write(TSignal signal)
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
        public IAsyncEnumerator<TSignal> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new AsyncEnumerator(this, _monitor, cancellationToken, _begBlock);
        }

        /// <summary>
        /// Provides an enumerator for a channel.
        /// </summary>
        class AsyncEnumerator : IAsyncEnumerator<TSignal>
        {

            readonly LocalChannel<TSignal> _channel;
            readonly AsyncMonitor _monitor;
            readonly CancellationToken _cancellationToken;

            Block _block;
            int _position;

            /// <summary>
            /// Initializes a new instance starting from the specified block.
            /// </summary>
            /// <param name="channel"></param>
            /// <param name="monitor"></param>
            /// <param name="cancellationToken"></param>
            /// <param name="initialBlock"></param>
            public AsyncEnumerator(LocalChannel<TSignal> channel, AsyncMonitor monitor, CancellationToken cancellationToken, Block initialBlock)
            {
                _channel = channel;
                _monitor = monitor;
                _cancellationToken = cancellationToken;
                _block = initialBlock;
                _position = -1;
            }

            /// <inheritdoc />
            public TSignal Current => _position >= 0 ? _block.Data[_position] : throw new InvalidOperationException();

            /// <inheritdoc />
            public async ValueTask<bool> MoveNextAsync()
            {
                try
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
                catch (OperationCanceledException)
                {
                    return false;
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
