using System;
using System.Diagnostics.CodeAnalysis;

using Gip.Abstractions;

namespace Gip.Hosting
{

    /// <summary>
    /// Default <see cref="IPipelineContext"/> implementation.
    /// </summary>
    public abstract class Pipeline : IPipelineContext
    {

        readonly FunctionContainer _functions;
        readonly ChannelContainer _channels;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public Pipeline()
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
        public IFunctionHandle CreateFunction(IFunctionContext function)
        {
            return _functions.Create(function);
        }

        /// <summary>
        /// Registers a channel for the given schema.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public IChannelHandle CreateChannel(ChannelSchema schema)
        {
            return _channels.Create(schema);
        }

        /// <inheritdoc />
        public abstract Uri GetFunctionUri(Guid functionId);

        /// <inheritdoc />
        public abstract Uri GetChannelUri(Guid channelId);

    }

}
