using System.Threading;
using System.Threading.Tasks;

namespace Gib.Core.Elements
{

    public interface IElement
    {

        /// <summary>
        /// Invoked to execute the element.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ExecuteAsync(CancellationToken cancellationToken);

    }

}
