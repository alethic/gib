using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;

namespace Gip.Hosting
{

    /// <summary>
    /// Holds a reference to a registered service in the service container.
    /// </summary>
    public class LocalFunction : IFunctionHandle
    {

        readonly LocalPipelineHost _host;
        readonly IFunctionContext _context;
        readonly Guid _id;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="context"></param>
        /// <param name="id"></param>
        internal LocalFunction(LocalPipelineHost host, IFunctionContext context, Guid id)
        {
            _host = host;
            _context = context;
            _id = id;
        }

        /// <inheritdoc />
        public FunctionSchema Schema => _context.Schema;

        /// <summary>
        /// Gets the registered <see cref="IFunctionContext"/>.
        /// </summary>
        public IFunctionContext Context => _context;

        /// <summary>
        /// Gets the ID of the function.
        /// </summary>
        public Guid Id => _id;

        /// <inheritdoc />
        public async ValueTask<ICallHandle> CallAsync(IServiceProvider services, ImmutableArray<SourceParameter> sources, CancellationToken cancellationToken)
        {
            var context = new LocalCallContext(_host, services, Context, sources);
            await context.StartAsync(cancellationToken);
            return context;
        }

    }

}