using System;

namespace Gip.Core
{

    /// <summary>
    /// Implementation of a sink pad.
    /// </summary>
    public class GipSinkPad : GipPad<GipSinkPadTemplate, GipSendPad>
    {

        public delegate GipPadLinkResult LinkFuncDelegate(GipSinkPad sinkPad, GipSendPad sendPad, GipElement element);
        public delegate GipPadRecvResult RecvFuncDelegate(GipSinkPad sinkPad, GipSendPad sendPad, GipElement element, object? data);

        /// <summary>
        /// Default implementation for the link function of a sink pad.
        /// </summary>
        /// <param name="sinkpad"></param>
        /// <param name="sendPad"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        static GipPadLinkResult DefaultLinkFunc(GipSinkPad sinkpad, GipSendPad sendPad, GipElement element)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Default implementation for the link function of a sink pad.
        /// </summary>
        /// <param name="sinkpad"></param>
        /// <param name="sendPad"></param>
        /// <param name="element"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        static GipPadRecvResult DefaultRecvFunc(GipSinkPad sinkpad, GipSendPad sendPad, GipElement element, object? data)
        {
            return GipPadRecvResult.Refused;
        }

        LinkFuncDelegate linkFunc;
        RecvFuncDelegate recvFunc;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="template"></param>
        internal GipSinkPad(GipSinkPadTemplate template) :
            base(template)
        {
            linkFunc = DefaultLinkFunc;
            recvFunc = DefaultRecvFunc;
        }

        /// <summary>
        /// Gets or sets the custom linking function, if any.
        /// </summary>
        public LinkFuncDelegate LinkFunc
        {
            get => linkFunc;
            set => linkFunc = value;
        }

        /// <summary>
        /// Gets or sets the custom receiving function, if any.
        /// </summary>
        public RecvFuncDelegate RecvFunc
        {
            get => recvFunc;
            set => recvFunc = value;
        }

        /// <summary>
        /// Implementation of <see cref="ActivateModeInternal"/> for <see cref="GipSendPad"/>.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="activate"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected internal override bool ActivateModeInternal(GipPadMode mode, bool activate)
        {
            throw new NotImplementedException();
        }

    }


}
