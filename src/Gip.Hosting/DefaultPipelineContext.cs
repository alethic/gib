using System;
using System.Diagnostics.CodeAnalysis;

using Gip.Abstractions;

namespace Gip.Hosting
{

    /// <summary>
    /// Default <see cref="IPipelineContext"/> implementation.
    /// </summary>
    public abstract class DefaultPipelineContext : IPipelineContext
    {

        readonly DefaultFunctionContainer _functions;
        readonly DefaultChannelContainer _channels;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public DefaultPipelineContext()
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
        internal DefaultFunction CreateFunction(IFunctionContext function)
        {
            return _functions.Create(function);
        }

        /// <summary>
        /// Registers a channel for the given schema.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        internal ChannelImpl CreateChannel(ChannelSchema schema)
        {
            return _channels.Create(schema);
        }

        /// <inheritdoc />
        IFunctionHandle IPipelineContext.CreateFunction(IFunctionContext function)
        {
            return CreateFunction(function);
        }

        /// <inheritdoc />
        public abstract FunctionReference GetFunctionReference(IFunctionHandle function);

        /// <inheritdoc />
        public abstract ChannelReference GetChannelReference(IChannelHandle channel);

    }

}
