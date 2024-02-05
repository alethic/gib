using System;
using System.Collections;
using System.Collections.Generic;
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
            protected internal set { SetName(value); }
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
        /// Sets the <see cref="Parent"/> property.
        /// </summary>
        /// <param name="parent"></param>
        protected virtual void SetParent(GipObject? parent)
        {
            using var _ = Lock();

            // parent already set
            if (this.parent == parent)
                return;
            if (this.parent != null)
                throw new GipException("Element already parented.");

            // obtain an optional lock against the new parent's tree
            var remoteLock = default(GipLock?);
            if (parent != null)
                remoteLock = parent.Lock();

            try
            {
                // validate that the existing links are correct
                CheckParentInvariants();

                var next = this.next;
                var prev = this.prev;

                // unlink from existing parent graph
                if (this.parent != null)
                {

                    // we had a prev node, so link the next node of our prev over us
                    if (prev != null)
                        prev.next = next;

                    // we had a next node, so link the prev node of our next over us
                    if (next != null)
                        next.prev = prev;

                    // parent down was previously pointing to us, now points to original next (new head)
                    if (this.parent.down == this)
                        this.parent.down = next;
                }

                // link into new parent structure
                if (parent != null)
                {
                    // parent sibling pointer is already present
                    if (parent.down != null)
                    {
                        Debug.Assert(parent.down.prev == null);
                        parent.down.prev = this; // previous sibling points back to us
                        this.next = parent.down; // we point to previous sibling
                    }

                    // new first sibling is us
                    parent.down = this;
                }

                // validate that we didn't break the links
                CheckParentInvariants();

                SetRoot(parent?.root ?? this); // reset root of all objects below us to new root or ourselves as root
                SetPropertyValue(nameof(Parent), ref this.parent, parent);
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
