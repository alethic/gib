using System;

namespace Gip.Core
{

    /// <summary>
    /// Describes a property value being changed in a <see cref="GipObject"/>.
    /// </summary>
    public class GipPropertyChangedEventArgs : GibPropertyChangedEventArgs<object>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="name"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        public GipPropertyChangedEventArgs(GipObject sender, string name, object? oldValue, object? newValue) :
            base(sender, name, oldValue, newValue)
        {

        }

    }

    /// <summary>
    /// Describes a property value being changed in a <see cref="GipObject"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GibPropertyChangedEventArgs<T> : GipEventArgs
    {

        readonly string name;
        readonly T? oldValue;
        readonly T? newValue;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="name"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        public GibPropertyChangedEventArgs(GipObject sender, string name, T oldValue, T newValue) :
            base(sender)
        {
            this.name = name;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        /// <summary>
        /// Gets the name of the changed property.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Gets the old value.
        /// </summary>
        public T? OldValue => oldValue;

        /// <summary>
        /// Gets the new value.
        /// </summary>
        public T? NewValue => newValue;

    }

}
