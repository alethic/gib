using System;
using System.Diagnostics.CodeAnalysis;

namespace Gip.Abstractions
{

    /// <summary>
    /// An <see cref="IPipelineContext"/> is reponsible for delivering call events to the appropriate element instance, managing
    /// signals stores, and forwarding outbound communication. A <see cref="IPipelineContext"/> is generally consumed within some sort of
    /// protocol hosting environment.
    /// </summary>
    public interface IPipelineContext
    {

        /// <summary>
        /// Gets a handle to the specified function.
        /// </summary>
        /// <param name="functionId"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        bool TryGetFunction(Guid functionId, [NotNullWhen(true)] out ILocalFunctionHandle? function);

        /// <summary>
        /// Gets a handle to the specified function.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        bool TryGetChannel(Guid channelId, [NotNullWhen(true)] out ILocalChannelHandle? channel);

        /// <summary>
        /// Creates a host to a local function.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        ILocalFunctionHandle CreateFunction(IFunctionContext function);

        /// <summary>
        /// Creates a host to a local channel.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        ILocalChannelHandle CreateChannel(ChannelSchema schema);

        /// <summary>
        /// Gets a serializable reference to the given function by ID.
        /// </summary>
        /// <param name="functionId"></param>
        /// <returns></returns>
        Uri GetLocalFunctionUri(Guid functionId);

        /// <summary>
        /// Gets a serializable reference to the given channel by ID.
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        Uri GetLocalChannelUri(Guid channelId);

        /// <summary>
        /// Gets a serializable reference to the given function.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        Uri GetFunctionUri(IFunctionHandle function);

        /// <summary>
        /// Gets a serializable reference to the given channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        Uri GetChannelUri(IChannelHandle channel);

        /// <summary>
        /// Gets a serializable reference to the given function.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        FunctionReference GetFunctionReference(IFunctionHandle function)
        {
            if (function is ILocalFunctionHandle local)
                return new FunctionReference(GetFunctionUri(local)) { Instance0 = function };
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a serializable reference to the given channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        ChannelReference GetChannelReference(IChannelHandle channel)
        {
            if (channel is ILocalChannelHandle local)
                return new ChannelReference(GetChannelUri(local)) { Instance0 = channel };
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a handle to the function represented by the reference.
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        IFunctionHandle ResolveFunction(FunctionReference reference)
        {
            if (reference.Instance0 is IFunctionHandle handle)
                return handle;
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a handle to the channel represented by the reference.
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        IChannelHandle ResolveChannel(ChannelReference reference)
        {
            if (reference.Instance0 is IChannelHandle handle)
                return handle;
            else
                throw new NotImplementedException();
        }

    }

}
