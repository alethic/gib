using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gib.Core.Elements
{

    public interface IValueBinding<T>
    {

        /// <summary>
        /// Gets the latest version of the value.
        /// </summary>
        /// <returns></returns>
        ValueTask<T> GetValueAsync(CancellationToken cancellationToken);

        IValueBinding<TTarget> Select<TTarget>(Func<T, TTarget> selector);

    }

}
