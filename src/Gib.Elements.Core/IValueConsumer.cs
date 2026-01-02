using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gib.Core.Elements
{

    public interface IValueConsumer<T>
    {

        /// <summary>
        /// Invokes the given action with the value on each change.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Listen(Action<T> action, CancellationToken cancellationToken);

    }

}
