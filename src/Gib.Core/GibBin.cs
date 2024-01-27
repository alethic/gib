using System.Collections.Generic;
using System.Linq;

namespace Gib.Core
{

    public abstract class GibBin : GibElement
    {

        readonly HashSet<GibElement> elements = new();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public GibBin()
        {

        }

        /// <summary>
        /// Gets the elements that are located inside this bin.
        /// </summary>
        public IReadOnlySet<GibElement> Elements => elements;

        /// <inheritdoc />
        protected override bool TryChangeState(GibElementState state)
        {
            lock (SyncRoot)
                return elements.All(i => i.TrySetTargetState(state));
        }

        /// <summary>
        /// Attempts to add the <see cref="GibElement"/> to the <see cref="GibBin"/>. If the <see cref="GibElement"/>
        /// already exists in the <see cref="GibBin"/> no action is taken. The <see cref="GibElement"/> must be in the
        /// <see cref="GibElementState.None"/> state without a current <see cref="GibBin"/>.
        /// </summary>
        /// <param name="element"></param>
        public void Add(GibElement element)
        {
            lock (SyncRoot)
            {
                if (TargetState == GibElementState.Running || State == GibElementState.Running)
                    throw new GibException($"{nameof(GibBin)} cannot be modified when in the '{nameof(GibElementState.Running)}' state.");

                if (element.TargetState != GibElementState.None || element.TargetState != GibElementState.None)
                    throw new GibException($"{nameof(GibElement)} cannot be added to when not in the '{nameof(GibElementState.None)}' state.");

                // set parent and add to collection
                if (elements.Add(element))
                    element.SetBin(this);

                // attempt to set the element to the target state
                element.TrySetTargetState(TargetState);
            }
        }

        /// <summary>
        /// Removes the <see cref="GibElement"/> from this <see cref="GibBin"/>. If the <see cref="GibElement"/> does not
        /// exist in this bin, no operation is taken.
        /// </summary>
        /// <param name="element"></param>
        public void Remove(GibElement element)
        {
            lock (SyncRoot)
            {
                if (TargetState == GibElementState.Running || State == GibElementState.Running)
                    throw new GibException($"{nameof(GibElement)} cannot be modified when in the '{nameof(GibElementState.Running)}' state.");

                // remove from collection and unset parent
                if (elements.Remove(element))
                    element.SetBin(null);
            }
        }

    }

}
