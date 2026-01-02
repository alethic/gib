using System;
using System.Diagnostics.CodeAnalysis;

namespace Gip.Abstractions
{

    /// <summary>
    /// An <see cref="IPipelineHost"/> is reponsible for delivering call events to the appropriate element instance, managing
    /// signals stores, and forwarding outbound communication. A <see cref="IPipelineHost"/> is generally consumed within some sort of
    /// protocol hosting environment.
    /// </summary>
    public interface IPipelineHost
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
        IFunctionHandle RegisterFunction(IFunctionContext function);

    }

}
