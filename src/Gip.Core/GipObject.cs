using System;
using System.Diagnostics;

namespace Gip.Core
{

    /// <summary>
    /// Base of all Gib objects with a required parent type.
    /// </summary>
    /// <typeparam name="TParent"></typeparam>
    public abstract class GipObject<TParent> : GipObject
        where TParent : GipObject
    {

        /// <summary>
        /// Gets the parent of this <see cref="TParent"/>.
        /// </summary>
        public new TParent? Parent
        {
            get { using (Lock()) { return (TParent?)base.Parent; } }
            protected internal set { base.Parent = value; }
        }


        /// <inheritdoc />
        protected override void SetParent(GipObject? parent)
        {
            if (parent is not null and not TParent)
                throw new GipException($"Parent of {GetType().Name} can only be a {typeof(TParent).Name}.");

            base.SetParent(parent);
        }

    }

    /// <summary>
    /// Base of all Gip objects.
    /// </summary>
    /// <remarks>
    /// Objects can be parented to other objects, as long as they accept the object as a parent. Each object also maintains an internal linked list of child objects.
    /// The <see cref="down"/> field records the head node of the child objects. Each child object has a <see cref="next"/> and <see cref="prev"/> field which determines
    /// its place as a child.
    /// 
    /// Objects maintain a global monitor lock within their graph by each holding a reference to the root node of the current graph, and locking around it.
    /// 
    /// Objects can raise events which are propigated up in the graph to their parents.
    /// </remarks>
    public abstract class GipObject
    {

        internal string name = string.Empty;
        internal GipObject? parent;
        internal GipObject root;
        internal GipObject? down;
        internal GipObject? next;
        internal GipObject? prev;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected GipObject()
        {
            root = this;
        }

        /// <summary>
        /// Obtains a disposable lock around this elements graph.
        /// </summary>
        /// <returns></returns>
        public GipLock Lock()
        {
            return new GipLock(root);
        }

        /// <summary>
        /// Raised when a Gip event is dispatched to the object.
        /// </summary>
        public event EventHandler<GipEventArgs>? Event;

        /// <summary>
        /// Raises a Gip event from this object.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnEvent(GipEventArgs args)
        {
            Event?.Invoke(this, args);
        }

        /// <summary>
        /// Raises a Gip event from this object.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void RaiseEvent(GipEventArgs args)
        {
            // lock for the duration of the event dispatch
            using var _ = Lock();

            // dispatch object to this object and all of its parents
            var p = this;
            while (p != null)
            {
                p.OnEvent(args);
                p = p.Parent;
            }
        }

        /// <summary>
        /// Raises an event when a property is changed.
        /// </summary>
        /// <param name="name"></param>
        protected virtual void RaisePropertyChanging(string name, object? oldValue, object? newValue)
        {
            RaiseEvent(new GipPropertyChangingEventArgs(this, name, oldValue, newValue));
        }

        /// <summary>
        /// Raises an event when a property is changed.
        /// </summary>
        /// <param name="name"></param>
        protected virtual void RaisePropertyChanged(string name, object? oldValue, object? newValue)
        {
            RaiseEvent(new GipPropertyChangedEventArgs(this, name, oldValue, newValue));
        }

        /// <summary>
        /// Sets the property value given by the specified field.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        protected void SetPropertyValue<T>(string name, ref T field, T value)
        {
            using var _ = Lock();

            if (Equals(field, value) == false)
            {
                var oldValue = field;
                RaisePropertyChanging(name, oldValue, value);
                field = value;
                RaisePropertyChanged(name, oldValue, value);
            }
        }

        /// <summary>
        /// Gets or sets the name of this object.
        /// </summary>
        public string Name
        {
            get { using (Lock()) { return name; } }
            set { SetName(value); }
        }

        /// <summary>
        /// Sets the <see cref="Name"/> property.
        /// </summary>
        /// <param name="name"></param>
        protected virtual void SetName(string name)
        {
            SetPropertyValue(nameof(Name), ref this.name, name);
        }

        /// <summary>
        /// Gets the parent of this <see cref="GipObject"/>.
        /// </summary>
        public GipObject? Parent
        {
            get { using (Lock()) { return parent; } }
            protected internal set { SetParent(value); }
        }

        /// <summary>
        /// Validates that the parent state is correct.
        /// </summary>
        [Conditional("DEBUG")]
        void CheckParentInvariants()
        {
            if (parent == null)
            {
                Debug.Assert(prev == null);
                Debug.Assert(next == null);
            }
            else
            {
                Debug.Assert(parent.down != null);

                if (parent.down == this)
                {
                    // if parent points to us, we cannot point to a previous element
                    Debug.Assert(prev == null);
                }
                else
                {
                    // if parent points to somebody else, we must not be a head node, and therefor must have a prev node
                    Debug.Assert(prev != null);
                }
            }

            if (next != null)
            {
                // if we have a next node, check its state
                Debug.Assert(next.prev == this);
                Debug.Assert(next.parent == parent);
            }

            if (prev != null)
            {
                // if we have a prev node, check its state
                Debug.Assert(prev.next == this);
                Debug.Assert(prev.parent == parent);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if this object can accept the given object as a child.
        /// </summary>
        /// <param name="obj"></param>
        protected virtual bool CanBeParentOf(GipObject obj)
        {
            return true;
        }

        /// <summary>
        /// Sets the <see cref="Parent"/> property.
        /// </summary>
        /// <param name="parent"></param>
        protected virtual void SetParent(GipObject? parent)
        {
            // ask parent if we can be it's child
            if (parent != null && parent.CanBeParentOf(this) == false)
                throw new GipException("Cannot parent this object to the specified object.");

            // lock current tree
            using var _ = Lock();

            // parent already set
            if (this.parent == parent)
                return;
            if (this.parent != null)
                throw new GipException("Element already parented.");

            // raise changing event on existing tree
            RaisePropertyChanging(nameof(Parent), this.parent, parent);

            // obtain a lock against the new parent's tree, if there is one
            var remoteLock = default(GipLock?);
            if (parent != null)
                remoteLock = parent.Lock();

            try
            {
                // validate that the existing links are correct
                CheckParentInvariants();

                var prevParent = this.parent;
                var prevNext = this.next;
                var prevPrev = this.prev;

                // unlink from existing parent graph
                if (this.parent != null)
                {
                    // we had a prev node, so link the next node of our prev over us
                    if (prevPrev != null)
                        prevPrev.next = prevNext;

                    // we had a next node, so link the prev node of our next over us
                    if (prevNext != null)
                        prevNext.prev = prevPrev;

                    // parent down was previously pointing to us, now points to original next (new head)
                    if (this.parent.down == this)
                        this.parent.down = prevNext;
                }

                // link into new parent structure
                if (parent != null)
                {
                    // update parent
                    this.parent = parent;

                    // parent sibling pointer is already present
                    if (this.parent.down != null)
                    {
                        Debug.Assert(this.parent.down.prev == null);
                        this.parent.down.prev = this; // previous sibling points back to us
                        this.next = this.parent.down; // we point to previous sibling
                    }

                    // new first sibling is us
                    this.parent.down = this;

                    // update root to new parents root
                    SetRoot(this.parent.root);
                }
                else
                {
                    // we are the root
                    SetRoot(this);
                }

                // validate that we didn't break the links
                CheckParentInvariants();

                // raise changed event against new parent tree
                RaisePropertyChanged(nameof(Parent), prevParent, parent);
            }
            finally
            {
                remoteLock?.Dispose();
            }
        }

        /// <summary>
        /// Sets the new root value for this object and any objects below it.
        /// </summary>
        /// <param name="root"></param>
        void SetRoot(GipObject root)
        {
            down?.SetRoot(root);
            next?.SetRoot(root);
            this.root = root;
        }

    }

}
