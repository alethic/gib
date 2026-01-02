using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BernhardHaus.Collections.WeakDictionary;

using Gip.Abstractions;

namespace Gip.Hosting
{

    /// <summary>
    /// Maintains a set of registered channels within a <see cref="IPipelineHost"/>.
    /// </summary>
    class LocalChannelContainer
    {

        readonly object _lock = new object();
        readonly LocalPipelineHost _host;
        readonly WeakDictionary<Guid, LocalChannel> _channelsById = new();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public LocalChannelContainer(LocalPipelineHost host)
        {
            _host = host;
        }

        /// <summary>
        /// Registers a channel for the specified schema.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public LocalChannel Create(ChannelSchema schema)
        {
            lock (_lock)
            {
                // each registered function gets a unique key
                var id = Guid.NewGuid();
                var hndl = new LocalChannel(this, schema, (IChannelStore)Activator.CreateInstance(typeof(InMemoryChannelStore<>).MakeGenericType(schema.Signal))!, id);

                if (_channelsById.TryAdd(id, hndl) == false)
                    throw new InvalidOperationException();

                return hndl;
            }
        }

        /// <summary>
        /// Gets a <see cref="IChannelHandle"/> for interacting with the specified channel.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool TryGetChannel(Guid id, [NotNullWhen(true)] out LocalChannel? handle)
        {
            lock (_lock)
            {
                // find by ID
                if (_channelsById.TryGetValue(id, out var h))
                {
                    handle = h;
                    return true;
                }

                handle = null;
                return false;
            }
        }

    }

}

