using System.Xml.Linq;

namespace Gib.Core
{

    /// <summary>
    /// A <see cref="GibElement"/> represents the basic building block of a <see cref="GibPipeline"/>.
    /// </summary>
    public abstract class GibElement : GibObject
    {

        object syncRoot = new object();

        GibElementState targetState;
        GibElementState state;
        GibBin? bin;

        readonly GibPadCollection pads = new GibPadCollection();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public GibElement()
        {

        }

        /// <summary>
        /// Object to synchronize access to internal state.
        /// </summary>
        protected internal object SyncRoot => syncRoot;

        /// <summary>
        /// Gets the parent bin of this element.
        /// </summary>
        public GibBin? Bin => bin;

        /// <summary>
        /// Throws an exception if the current state of the element does not match the specified state.
        /// </summary>
        /// <param name="state"></param>
        /// <exception cref="GibException"></exception>
        internal protected void AssertState(GibElementState state)
        {
            lock (syncRoot)
                if (this.state != state)
                    throw new GibException($"{nameof(GibElement)} not in required state '{state}'.");
        }

        /// <summary>
        /// Gets or sets the current target state of the element.
        /// </summary>
        public GibElementState TargetState => targetState;

        /// <summary>
        /// Gets the current actual state of this element.
        /// </summary>
        public GibElementState State => state;

        /// <summary>
        /// Gets the pads of the element.
        /// </summary>
        public GibPadCollection Pads => pads;

        /// <summary>
        /// Raised when an event occurs related to the element.
        /// </summary>
        public event GibElementEventHandler? OnEvent;

        /// <summary>
        /// Sets the bin of this element. This method should only be invoked by <see cref="GibBin"/>.
        /// </summary>
        /// <param name="bin"></param>
        protected internal virtual void SetBin(GibBin? bin)
        {
            // already a member of this bin, ignore
            if (this.bin == bin)
                return;

            // a member of a different bin
            if (bin != null && this.bin != null)
                throw new GibException($"This {nameof(GibElement)} is already a member of a {nameof(GibBin)}.");

            // set and raise event
            var oldBin = this.bin;
            this.bin = bin;
            OnEvent?.Invoke(this, new GibElementEventBinArgs(this, oldBin, this.bin));
        }

        /// <summary>
        /// Invoked by the user to set the desired state.
        /// </summary>
        /// <param name="targetState"></param>
        public bool TrySetTargetState(GibElementState targetState)
        {
            lock (syncRoot)
            {
                // pipeline is the only element that can change state without a bin
                if (this is not GibPipeline && bin == null)
                    throw new GibException($"Element requires a configured {nameof(GibBin)} to change state.");

                // update target state
                this.targetState = targetState;

                // we haven't yet proceeded to the target state, so try to change
                if (state != targetState)
                    return TryChangeStateInternal();
            }

            // we did no change, indicate success
            return true;
        }

        /// <summary>
        /// Invoked when an element should transition to the <see cref="GibElementState.Running"/> state.
        /// </summary>
        bool TryChangeStateInternal()
        {
            try
            {
                if (TryChangeState(targetState))
                {
                    var oldState = state;
                    state = targetState;

                    try
                    {
                        OnEvent?.Invoke(this, new GibElementEventStateChangedArgs(this, oldState, state));
                    }
                    catch
                    {
                        // TODO log this, but proceed with state change
                    }
                }
            }
            catch
            {
                // TODO log this, but indicate we failed
                return false;
            }

            return true;
        }

        /// <summary>
        /// Invoked when an element should transition to the specified state.
        /// </summary>
        protected abstract bool TryChangeState(GibElementState state);

    }

}
