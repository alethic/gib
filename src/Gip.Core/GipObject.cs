using System;

namespace Gip.Core
{

    /// <summary>
    /// Base of all Gib objects with a required parent type.
    /// </summary>
    /// <typeparam name="TParent"></typeparam>
    public abstract class GipObject<TParent> : GipObject
        where TParent : GipObject
    {

        /// <inheritdoc />
        protected override void SetParent(GipObject? parent)
        {
            if (parent is not null and not TParent)
                throw new GipException($"Parent of {GetType().Name} can only be a {typeof(TParent).Name}.");

            base.SetParent(parent);
        }

    }

    /// <summary>
    /// Base of all Gib objects.
    /// </summary>
    public abstract class GipObject
    {

        string name = string.Empty;
        GipObject? root;
        GipObject? parent;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected GipObject()
        {

        }

        /// <summary>
        /// Obtains a disposable lock around this elements graph.
        /// </summary>
        /// <returns></returns>
        public GipLock Lock()
        {
            // seek upwards from this until no parent found
            var p = this;
            var i = p.Parent;
            while (i != null)
            {
                p = i;
                i = i.Parent;
            }

            // return new lock
            return new GipLock(p);
        }

        /// <summary>
        /// Raised when a Gip event is dispatched to the object.
        /// </summary>
        public event EventHandler<GipEventArgs> Event2;

        /// <summary>
        /// Raises a Gip event from this object.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnEvent(GipEventArgs args)
        {
            using var lck = Lock();

            // dispatch object to this object and all of its parents
            var p = this;
            while (p != null)
            {
                p.OnEvent(args);
                p = p.Parent;
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="name"></param>
        protected virtual void OnPropertyChanged(string name, object? oldValue, object? newValue)
        {
            PropertyChanged?.Invoke(this, new GipPropertyChangedEventArgs(name, oldValue, newValue));
        }

        /// <summary>
        /// Sets the property value given by the specified field.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        protected void SetPropertyValue<T>(string name, ref T field, T value)
        {
            var valueChanged = false;
            T? oldValue = default;

            lock (this)
            {
                if (Equals(field, value) == false)
                {
                    valueChanged = true;
                    oldValue = field;
                    field = value;
                }
            }

            if (valueChanged)
                OnPropertyChanged(name, oldValue, value);
        }

        /// <summary>
        /// Gets or sets the name of this object.
        /// </summary>
        public virtual string Name
        {
            get { lock (this) { return name; } }
            protected internal set { SetName(value); }
        }

        /// <summary>
        /// Sets the <see cref="Name"/> property.
        /// </summary>
        /// <param name="value"></param>
        protected virtual void SetName(string value)
        {
            SetPropertyValue(nameof(Name), ref this.name, value);
        }

        /// <summary>
        /// Gets the parent of this <see cref="GipObject"/>.
        /// </summary>
        public virtual GipObject? Parent
        {
            get { lock (this) { return parent; } }
            protected internal set { SetParent(value); }
        }

        /// <summary>
        /// Sets the <see cref="Parent"/> property.
        /// </summary>
        /// <param name="value"></param>
        protected virtual void SetParent(GipObject? value)
        {
            SetPropertyValue(nameof(Parent), ref parent, value);
        }

    }

}
