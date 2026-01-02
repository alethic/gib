using System;
using System.Diagnostics.CodeAnalysis;

using Gip.Abstractions;

namespace Gip.Hosting
{

    /// <summary>
    /// Default <see cref="IPipelineHost"/> implementation.
    /// </summary>
    public class LocalPipelineHost : IPipelineHost
    {

        readonly LocalFunctionContainer _functions;
        readonly LocalChannelContainer _channels;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public LocalPipelineHost()
        {
            _functions = new(this);
            _channels = new(this);
        }

        /// <summary>
        /// Attempts to get a reference to an existing function.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="registration"></param>
        /// <returns></returns>
        public bool TryGetFunction(Guid id, [NotNullWhen(true)] out IFunctionHandle? registration)
        {
            if (_functions.TryGetFunction(id, out var h))
            {
                registration = h;
                return true;
            }

            registration = null;
            return false;
        }

        /// <summary>
        /// Attempts to get a reference to an existing function.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="registration"></param>
        /// <returns></returns>
        public bool TryGetChannel(Guid id, [NotNullWhen(true)] out IChannelHandle? registration)
        {
            if (_channels.TryGetChannel(id, out var h))
            {
                registration = h;
                return true;
            }

            registration = null;
            return false;
        }

        /// <summary>
        /// Registers a channel for the given schema.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public LocalFunction CreateFunction(IFunctionContext function)
        {
            return _functions.Create(function);
        }

        /// <summary>
        /// Registers a channel for the given schema.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public LocalChannel CreateChannel(ChannelSchema schema)
        {
            return _channels.Create(schema);
        }

        /// <inheritdoc />
        IFunctionHandle IPipelineHost.RegisterFunction(IFunctionContext function)
        {
            return CreateFunction(function);
        }

    }

}
