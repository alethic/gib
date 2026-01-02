namespace Gib.Core.Elements
{

    public interface IValueProducer<T>
    {

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        void Set(T value);

        /// <summary>
        /// Binds the value.
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        void Bind(IValueBinding<T> binding);

    }

}
