using System;
using System.Collections.Generic;
using System.Linq;

namespace Gip.Core
{

    public abstract class GipBin : GipElement
    {

        readonly HashSet<GipElement> elements = new();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="factory"></param>
        protected GipBin(GipElementFactory factory) :
            base(factory)
        {

        }

        /// <summary>
        /// Gets the elements that are located inside this bin.
        /// </summary>
        public IReadOnlySet<GipElement> Elements => elements;

        /// <inheritdoc />
        protected override bool CanBeParentOf(GipObject obj)
        {
            // we accept elements and pads
            return obj is GipElement || base.CanBeParentOf(obj);
        }

        /// <inheritdoc />
        protected override bool TryChangeState(GipState state)
        {
            lock (this)
                return elements.All(i => i.TrySetTargetState(state));
        }

        /// <summary>
        /// Attempts to add the <see cref="GipElement"/> to the <see cref="GipBin"/>. If the <see cref="GipElement"/>
        /// already exists in the <see cref="GipBin"/> no action is taken. The <see cref="GipElement"/> must be in the
        /// <see cref="GipState.None"/> state without a current <see cref="GipBin"/>.
        /// </summary>
        /// <param name="element"></param>
        public void AddElement(GipElement element)
        {
            lock (this)
            {
                if (TargetState == GipState.Running || State == GipState.Running)
                    throw new GipException($"{nameof(GipBin)} cannot be modified when in the '{nameof(GipState.Running)}' state.");

                if (element.TargetState != GipState.Null || element.TargetState != GipState.Null)
                    throw new GipException($"{nameof(GipElement)} cannot be added to when not in the '{nameof(GipState.Null)}' state.");

                // set parent and add to collection
                if (elements.Add(element))
                    element.Parent = this;

                // attempt to set the element to the target state
                element.TrySetTargetState(TargetState);
            }
        }

        /// <summary>
        /// Removes the <see cref="GipElement"/> from this <see cref="GipBin"/>. If the <see cref="GipElement"/> does not
        /// exist in this bin, no operation is taken.
        /// </summary>
        /// <param name="element"></param>
        public void RemoveElement(GipElement element)
        {
            lock (this)
            {
                if (TargetState == GipState.Running || State == GipState.Running)
                    throw new GipException($"{nameof(GipElement)} cannot be modified when in the '{nameof(GipState.Running)}' state.");

                // remove from collection and unset parent
                if (elements.Remove(element))
                    element.Parent = null;
            }
        }

        /// <summary>
        /// Links <paramref name="src"/> to <paramref name="dst"/>. The link must be from source to destination;
        /// the other direction will not be tried. The function looks for existing pads that aren't linked yet. It will
        /// request new pads if necessary. Such pads need to be released manually when unlinking. If multiple links
        /// are possible, only one is established.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public void Link(GipElement src, GipElement dst)
        {
            lock (this)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Links <paramref name="src"/> to <paramref name="dst"/> using the given caps as filtercaps. The link must
        /// be from source to destination; the other direction will not be tried. The function looks for existing pads
        /// that aren't linked yet. It will request new pads if necessary. If multiple links are possible, only one
        /// is established.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public void Link(GipElement src, GipElement dst, GipCapList filter)
        {
            lock (this)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Chain together a series of elements. Make sure you have added your elements to a bin or pipeline before
        /// trying to link them.
        /// </summary>
        public void Link(GipElement element1, GipElement element2, params GipElement[] more)
        {
            lock (this)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Links the two pads of the source and destination elements.
        /// </summary>
        public void Link(GipPad src, GipPad dst)
        {
            lock (this)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Links the two pads of the source and destination elements.
        /// </summary>
        public void Link(GipPad src, GipPad dst, GipCapList filter)
        {
            lock (this)
            {
                throw new NotImplementedException();
            }
        }

    }

}
