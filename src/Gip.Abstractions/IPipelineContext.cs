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
        bool TryGetFunction(Guid functionId, [NotNullWhen(true)] out IFunctionHandle? function);

        /// <summary>
        /// Gets a handle to the specified function.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        bool TryGetChannel(Guid channelId, [NotNullWhen(true)] out IChannelHandle? channel);

        /// <summary>
        /// Registers the given function and returns a handle.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        IFunctionHandle CreateFunction(IFunctionContext function);

        /// <summary>
        /// Gets a serializable <see cref="FunctionReference"/> for the given <see cref="IFunctionHandle"/>.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        FunctionReference GetFunctionReference(IFunctionHandle function);

        /// <summary>
        /// Gets a serializable <see cref="ChannelReference"/> for the given <see cref="IChannelHandle"/>.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        ChannelReference GetChannelReference(IChannelHandle channel);

    }

}
