using System.Threading;
using System.Threading.Tasks;

namespace Gip.Abstractions
{

    /// <summary>
    /// Describes a channel made available as an output for a function call.
    /// </summary>
    public abstract class OutputBinding : Binding
    {

        /// <summary>
        /// Opens a channel for writing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract ValueTask<IChannelWriter<T>> OpenAsync<T>(CancellationToken cancellationToken);

    }

}
