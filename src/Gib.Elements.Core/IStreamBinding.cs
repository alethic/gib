namespace Gib.Core.Elements
{

    public interface IStreamBinding<T>
    {

        /// <summary>
        /// Consumes the stream binding.
        /// </summary>
        /// <returns></returns>
        IStreamConsumer<T> Consume();

    }

}
