using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BernhardHaus.Collections.WeakDictionary;

using Gip.Abstractions;

namespace Gip.Hosting
{

    /// <summary>
    /// Maintains a set of registered functions within a <see cref="Pipeline"/>.
    /// </summary>
    class FunctionContainer
    {

        readonly object _lock = new object();
        readonly Pipeline _host;
        readonly WeakDictionary<Guid, FunctionImpl> _functionsById = new();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public FunctionContainer(Pipeline host)
        {
            _host = host;
        }

        /// <summary>
        /// Registers the service within the container. Keeping an instance of the handle keeps the registration alive.
        /// </summary>
        /// <param name="context"></param>
        public FunctionImpl Create(IFunctionContext context)
        {
            lock (_lock)
            {
                // each registered function gets a unique key
                var id = Guid.NewGuid();

                // start the run task of the context, which should process context events
                var hndl = new FunctionImpl(_host, context, id);

                try
                {
                    // id can only be added once, but this does keep the context alive
                    if (_functionsById.TryAdd(id, hndl) == false)
                        throw new InvalidOperationException();
                }
                catch (Exception)
                {
                    _functionsById.Remove(id);
                }

                return hndl;
            }
        }

        /// <summary>
        /// Gets a <see cref="FunctionImpl"/> for interacting with the specified function.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool TryGetFunction(Guid id, [NotNullWhen(true)] out FunctionImpl? handle)
        {
            lock (_lock)
            {
                // find by ID
                if (_functionsById.TryGetValue(id, out var h))
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

