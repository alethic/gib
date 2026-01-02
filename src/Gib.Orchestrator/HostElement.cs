using System.Threading;
using System.Threading.Tasks;

using Gib.Core;
using Gib.Core.Elements;

namespace Gib.Orchestrator
{

    [Element]
    class HostElement : ElementBase, IElementWithProxy<IHostProxy>
    {

        readonly IHost _host;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public HostElement(IElementContext context, IHost host) :
            base(context)
        {
            _host = host;
        }

        /// <summary>
        /// Gets the name of the host.
        /// </summary>
        [Property("name")]
        public string Name => _host.Name;

        /// <inheritdoc />
        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

    }

}
