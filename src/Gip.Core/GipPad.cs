using System;

namespace Gip.Core
{

    /// <summary>
    /// Represents a connection point on a <see cref="GipElement"/>.
    /// </summary>
    /// <typeparam name="TTemplate"></typeparam>
    /// <typeparam name="TPeer"></typeparam>
    public abstract class GipPad<TTemplate, TPeer> : GibPad
        where TTemplate : GipPadTemplate
        where TPeer : GibPad
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="template"></param>
        protected internal GipPad(TTemplate template) :
            base(template)
        {

        }

        /// <summary>
        /// Gets the template of this pad.
        /// </summary>
        public new TTemplate Template
        {
            get => (TTemplate)base.Template;
        }

        /// <summary>
        /// Gets or sets the peer pad of this pad.
        /// </summary>
        public new TPeer? Peer
        {
            get => (TPeer?)base.Peer;
            internal set => base.Peer = value;
        }

        /// <inheritdoc />
        protected override void SetPeer(GibPad? peer)
        {
            if (peer is not null and not TPeer)
                throw new GipException($"Peer of {GetType().Name} can only be a {typeof(TPeer).Name}.");

            base.SetPeer(peer);
        }

    }

    /// <summary>
    /// Represents a connection point on a <see cref="GipElement"/>.
    /// </summary>
    public abstract class GibPad : GipObject
    {

        /// <summary>
        /// Check that pads does not have any exisiting links and that hierarchy is valid for linking.
        /// </summary>
        /// <param name="sendPad"></param>
        /// <param name="sinkPad"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static GipPadLinkResult CheckLinkRelations(GipSendPad sendPad, GipSinkPad sinkPad, GipPadLinkFlags flags)
        {
            if (sendPad.Peer != null)
                return GipPadLinkResult.WasLinked;
            if (sinkPad.Peer != null)
                return GipPadLinkResult.WasLinked;

            // optionally check the hierarchy
            if ((flags & GipPadLinkFlags.CheckHierarchy) != 0)
                if (CheckLinkHierarchy(sendPad, sinkPad) == false)
                    return GipPadLinkResult.WrongHierarchy;

            return GipPadLinkResult.Ok;
        }

        /// <summary>
        /// Check if the grandparents of both pads are the same. This check is required so that we don't try to link
        /// pads from elements in different bins without ghostpads.
        /// </summary>
        /// <param name="sendPad"></param>
        /// <param name="sinkPad"></param>
        /// <returns></returns>
        static bool CheckLinkHierarchy(GipSendPad sendPad, GipSinkPad sinkPad)
        {
            var sendElement = sendPad.Element;
            var sinkElement = sinkPad.Element;

            // if one of the pads has no hierarchy we allow the link
            if (sendElement == null || sinkElement == null)
                return true;

            // we have a loop
            if (sendElement == sinkElement)
                return false;

            // if they both have a parent, we check the grandparents
            var sendBin = (GipBin?)sendElement?.Parent;
            var sinkBin = (GipBin?)sinkElement?.Parent;
            if (sendBin != sinkBin)
                return false;

            // no failed conditions
            return true;
        }

        /// <summary>
        /// Get the caps from both pads and see if the intersection is not empty.
        /// </summary>
        /// <param name="sendPad"></param>
        /// <param name="sinkPad"></param>
        /// <returns></returns>
        static bool CheckLinkCompatible(GipSendPad sendPad, GipSinkPad sinkPad, GipPadLinkFlags flags)
        {
            // check nothing: success
            if ((flags & (GipPadLinkFlags.CheckCaps | GipPadLinkFlags.CheckTemplateCaps)) == 0)
                return true;

            lock (sendPad)
            {
                lock (sinkPad)
                {
                    GipCap[]? sendCaps = null;
                    GipCap[]? sinkCaps = null;

                    // expensive caps checking takes priority over only checking template caps
                    if ((flags & GipPadLinkFlags.CheckCaps) != 0)
                    {
                        sendCaps = sendPad.QueryCaps(null);
                        sinkCaps = sinkPad.QueryCaps(null);
                    }
                    else
                    {
                        // if one of the two pads doesn't have a template, consider the intersection as valid
                        if (sendPad.Template == null || sinkPad.Template == null)
                            return true;

                        sendCaps = sendPad.Template.Caps;
                        sinkCaps = sinkPad.Template.Caps;
                    }

                    // if either pad is missing caps there is no possible intersection
                    if (sendCaps == null || sendCaps.Length == 0)
                        return false;
                    if (sinkCaps == null || sinkCaps.Length == 0)
                        return false;

                    // check if resulting caps can intersect
                    return GipCap.CanIntersect(sendCaps, sinkCaps);
                }
            }
        }

        /// <summary>
        /// Prepare the two pads for linking.
        /// </summary>
        /// <param name="sendPad"></param>
        /// <param name="sinkPad"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        static GipPadLinkResult PrepareLink(GipSendPad sendPad, GipSinkPad sinkPad, GipPadLinkFlags flags)
        {
            lock (sendPad)
            {
                lock (sinkPad)
                {
                    // check pads state, not already linked and correct hierachy
                    var r = CheckLinkRelations(sendPad, sinkPad, flags);
                    if (r != GipPadLinkResult.Ok)
                        return r;

                    // check pad caps for non - empty intersection
                    if (CheckLinkCompatible(sendPad, sinkPad, flags) == false)
                        return GipPadLinkResult.NoFormat;
                }
            }

            return GipPadLinkResult.Ok;
        }

        /// <summary>
        /// Get the caps from both pads and see if the intersection is not empty.
        /// </summary>
        /// <param name="sendPad"></param>
        /// <param name="sinkPad"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static bool CanLink(GipSendPad sendPad, GipSinkPad sinkPad, GipPadLinkFlags flags)
        {
            return PrepareLink(sendPad, sinkPad, flags) == GipPadLinkResult.Ok;
        }

        /// <summary>
        /// Links the source pad and the sink pad.
        /// </summary>
        /// <param name="sendPad"></param>
        /// <param name="sinkPad"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static GipPadLinkResult Link(GipSendPad sendPad, GipSinkPad sinkPad, GipPadLinkFlags flags)
        {
            // TODO notify parent of linking attempt
            // https://github.com/GStreamer/gstreamer/blob/b13ea8514002665be793ed130bf4a6ec400a2e89/subprojects/gstreamer/gst/gstpad.c#L2545

            GipPadLinkResult r;

            lock (sendPad)
            {
                lock (sinkPad)
                {
                    // prepare the pads to be linked
                    r = PrepareLink(sendPad, sinkPad, flags);
                    if (r != GipPadLinkResult.Ok)
                        return r;

                    //  must set peers before calling the link function
                    sendPad.Peer = sinkPad;
                    sinkPad.Peer = sendPad;

                    // pads have a custom link function
                    var sendFunc = sendPad.GetLinkFunc();
                    var sinkFunc = sinkPad.GetLinkFunc();
                    if (sendFunc != null || sinkFunc != null)
                    {
                        if (sendFunc != null)
                        {
                            if (sendPad.Element != null)
                                r = sendFunc(sendPad, sinkPad, sendPad.Element);
                        }
                        else if (sinkFunc != null)
                        {
                            if (sinkPad.Element != null)
                                r = sinkFunc(sinkPad, sendPad, sinkPad.Element);
                        }

                        // check if the same pads are linked still
                        if (sendPad.Peer != sinkPad || sinkPad.Peer != sendPad)
                            return GipPadLinkResult.WasLinked;

                        // failure occurred calling custom funcs, unset peers and exit
                        if (r != GipPadLinkResult.Ok)
                        {
                            sendPad.SetPeer(null);
                            sinkPad.SetPeer(null);
                            return r;
                        }
                    }
                }
            }

            // fire off a signal to each of the pads telling them that they've been linked
            sendPad.OnLinkEvent(GipPadLinkEventType.Linked, sinkPad);
            sinkPad.OnLinkEvent(GipPadLinkEventType.Linked, sendPad);

            // success
            return GipPadLinkResult.Ok;
        }

        /// <summary>
        /// Links the source pad and the sink pad.
        /// </summary>
        /// <param name="sendPad"></param>
        /// <param name="sinkPad"></param>
        /// <returns></returns>
        public static GipPadLinkResult Link(GipSendPad sendPad, GipSinkPad sinkPad)
        {
            return Link(sendPad, sinkPad, GipPadLinkFlags.Full);
        }

        GipPadTemplate template;
        GipPadActivateFuncDelegate activateFunc;
        GipPadFlags flags;
        GibPad? peer;
        GipCap[]? caps;
        GipPadMode mode;
        object? userState;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="template"></param>
        protected internal GibPad(GipPadTemplate template)
        {
            this.template = template;
            this.activateFunc = DefaultActivateFunc;
        }

        /// <inheritdoc />
        protected override void OnPropertyChanged(string name, object? oldValue, object? newValue)
        {
            base.OnPropertyChanged(name, oldValue, newValue);

            switch (name)
            {
                case nameof(Parent):
                    OnPropertyChanged(nameof(Element), oldValue, newValue);
                    break;
            }
        }

        /// <inheritdoc />
        protected override void SetParent(GipObject? parent)
        {
            if (parent is not null and not GipElement)
                throw new GipException($"Parent of {GetType().Name} can only be a {typeof(GipElement).Name}.");

            base.SetParent(parent);
        }

        /// <summary>
        /// Gets the parent bin of this element.
        /// </summary>
        public GipElement? Element
        {
            get => (GipElement?)Parent;
            internal set => Parent = value;
        }

        /// <summary>
        /// Gets the template of this pad.
        /// </summary>
        public virtual GipPadTemplate Template
        {
            get => template;
        }

        /// <summary>
        /// Sets the specified flags. This method is not synchronzied.
        /// </summary>
        /// <param name="flags"></param>
        internal void SetFlags(GipPadFlags flags)
        {
            this.flags |= flags;
        }

        /// <summary>
        /// Unsets the specifid flags. This method is not synchronized.
        /// </summary>
        /// <param name="flags"></param>
        internal void UnsetFlags(GipPadFlags flags)
        {
            this.flags &= ~flags;
        }

        /// <summary>
        /// Gets or sets the peer of this pad.
        /// </summary>
        public virtual GibPad? Peer
        {
            get { lock (this) { return peer; } }
            internal set { SetPeer(value); }
        }

        /// <summary>
        /// Sets the peer of this pad.
        /// </summary>
        /// <param name="peer"></param>
        protected virtual void SetPeer(GibPad? peer)
        {
            SetPropertyValue(nameof(Peer), ref peer, peer);
        }

        /// <summary>
        /// Gets or sets the mode of the pad.
        /// </summary>
        public GipPadMode Mode
        {
            get { lock (this) { return mode; } }
            set { SetMode(value); }
        }

        /// <summary>
        /// Sets the mode of this pad.
        /// </summary>
        /// <param name="mode"></param>
        protected virtual void SetMode(GipPadMode mode)
        {
            SetPropertyValue(nameof(Mode), ref mode, mode);
        }

        /// <summary>
        /// Gets the negotiated caps of the pad.
        /// </summary>
        public GipCap[]? Caps => caps;

        /// <summary>
        /// Gets the set of caps on this pad which should be queryable.
        /// </summary>
        /// <returns></returns>
        GipCap[] GetQueryableCaps()
        {
            lock (this)
            {
                var isFixed = (flags & GipPadFlags.Fixed) != 0;

                // fixed caps, try the negotiated caps first
                if (isFixed)
                {
                    var caps = Caps;
                    if (caps != null)
                        return caps;
                }

                // next, check the template caps
                if (Template != null && Template.Caps != null)
                    return Template.Caps;

                // template had no caps, and we are not fixed
                if (isFixed == false)
                {
                    var caps = Caps;
                    if (caps != null)
                        return caps;
                }
            }

            // this almost never happens
            return GipCap.Any;
        }

        /// <summary>
        /// Queries for the capabilities of the pad that intersect with the given filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual GipCap[] QueryCaps(GipCap[]? filter)
        {
            var caps = GetQueryableCaps();

            // apply filter if present
            if (filter != null)
                caps = GipCap.Intersect(filter, caps, GipCapIntersectMode.First);

            return caps;
        }

        /// <inheritdoc />
        public object? UserState
        {
            get => userState;
            set => userState = value;
        }

        /// <summary>
        /// Raised when a link event occurs on the pad.
        /// </summary>
        public event EventHandler<GipPadLinkEventArgs>? LinkEvent;

        /// <summary>
        /// Invoke to raise a link event on the pad.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="peer"></param>
        internal void OnLinkEvent(GipPadLinkEventType eventType, GibPad? peer)
        {
            LinkEvent?.Invoke(this, new GipPadLinkEventArgs(this, eventType, peer));
        }

        /// <summary>
        /// Gets or sets the custom activate function, if any.
        /// </summary>
        public GipPadActivateFuncDelegate ActivateFunc
        {
            get => activateFunc;
            set => activateFunc = value;
        }

        /// <summary>
        /// Default 'ActivateFunc' implementation.
        /// </summary>
        protected bool DefaultActivateFunc()
        {
            return ActivateModeInternal(GipPadMode.Push, true); ;
        }

        /// <summary>
        /// Internal implementation of the default activate by mode.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="activate"></param>
        internal protected abstract bool ActivateModeInternal(GipPadMode mode, bool activate);

        /// <summary>
        /// Activates or deactivates the given pad.
        /// </summary>
        /// <param name="active"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal bool SetActive(bool active)
        {
            GipPadMode old;
            GipElement? parent;

            lock (this)
            {
                old = mode;
                parent = Element;
                if (parent == null)
                    return false;
            }

            if (active)
            {
                if (old == GipPadMode.None)
                {
                    return ActivateFunc();
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (old == GipPadMode.None)
                {
                    return true;
                }
                else
                {
                    return ActivateModeInternal(old, false);
                }
            }
        }

    }

}
