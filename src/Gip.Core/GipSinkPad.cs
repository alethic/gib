using System;

namespace Gip.Core
{

    /// <summary>
    /// Implementation of a sink pad.
    /// </summary>
    public class GipSinkPad : GipPad<GipSinkPadTemplate, GipSendPad>
    {

        public delegate GipPadLinkResult LinkFuncDelegate(GipSinkPad sinkPad, GipSendPad sendPad, GipElement element);

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

        LinkFuncDelegate linkFunc;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="template"></param>
        internal GipSinkPad(GipSinkPadTemplate template) :
            base(template)
        {
            linkFunc = DefaultLinkFunc;
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
