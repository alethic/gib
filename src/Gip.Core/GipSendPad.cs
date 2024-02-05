using System;

namespace Gip.Core
{

    /// <summary>
    /// Implementation of a send pad.
    /// </summary>
    public class GipSendPad : GipPad<GipSendPadTemplate, GipSinkPad>
    {

        public delegate GipPadLinkResult LinkFuncDelegate(GipSendPad sendPad, GipSinkPad sinkPad, GipElement element);

        /// <summary>
        /// Default implementation for the link function of a send pad.
        /// </summary>
        /// <param name="sendPad"></param>
        /// <param name="sinkpad"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        static GipPadLinkResult DefaultLinkFunc(GipSendPad sendPad, GipSinkPad sinkpad, GipElement element)
        {
            throw new NotImplementedException();
        }

        LinkFuncDelegate linkFunc;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="template"></param>
        internal GipSendPad(GipSendPadTemplate template) :
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
        /// <param name="active"></param>
        /// <returns></returns>
        protected internal override bool ActivateModeInternal(GipPadMode mode, bool active)
        {
            throw new NotImplementedException();
        //    var res = false;

        //    var oldMode = Mode;
        //    var newMode = active ? mode : GipPadMode.None;
        //    if (oldMode == null)
        //        goto was_ok;

        //exit_success:
        //    res = true;

        //    /* Clear sticky flags on deactivation */
        //    if (active == false)
        //        lock (this)
        //            UnsetFlags(GipPadFlags.NeedReconfigure);

        //exit:
        //    return res;

        //was_ok:
        //        goto exit_success;
        //deactivate_failed:
        //        goto exit;
        //peer_failed:
        //        goto exit;
        //    }
        //not_linked:
        //        goto exit;
        //failure:
        //    {
        //        GipSendPad(pad);
        //        GipSendPad(GST_CAT_PADS, pad, "failed to %s in %s mode",
        //            active ? "activate" : "deactivate", gst_pad_mode_get_name(mode));
        //        GipSendPad(pad);
        //        GipSendPad(pad) = old;
        //        pad->priv->in_activation = FALSE;
        //        GipSendPad(&pad->priv->activation_cond);
        //        GipSendPad(pad);
        //        goto exit;
        //    }
        }

    }

}
