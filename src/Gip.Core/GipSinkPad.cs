using System;

namespace Gip.Core
{

    /// <summary>
    /// Implementation of a sink pad.
    /// </summary>
    public class GipSinkPad : GipPad<GipSinkPadTemplate, GipSendPad>
    {

        public delegate GipPadLinkResult LinkFunc(GipSinkPad sinkPad, GipSendPad sendPad, GipElement element);
        public delegate bool ActivateFunc(GipSinkPad sinkPad, GipElement element);

        LinkFunc? linkFunc;
        ActivateFunc? activateFunc;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="template"></param>
        internal GipSinkPad(GipSinkPadTemplate template) :
            base(template)
        {

        }

        /// <summary>
        /// Gets the custom linking function, if any.
        /// </summary>
        internal LinkFunc? GetLinkFunc() => linkFunc;

        /// <summary>
        /// Gets the custom linking function, if any.
        /// </summary>
        internal ActivateFunc? GetActivateFunc() => activateFunc;

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
