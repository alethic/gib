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
    class FunctionImpl : IFunctionHandle
    {

        readonly Pipeline _pipeline;
        readonly IFunctionContext _context;
        readonly Guid _id;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="context"></param>
        /// <param name="id"></param>
        internal FunctionImpl(Pipeline pipeline, IFunctionContext context, Guid id)
        {
            _pipeline = pipeline;
            _context = context;
            _id = id;
        }

        /// <inheritdoc />
        public FunctionSchema Schema => _context.Schema;

        /// <summary>
        /// Gets the registered <see cref="IFunctionContext"/>.
        /// </summary>
        public IFunctionContext Context => _context;

        /// <inheritdoc />
        public Guid Id => _id;

        /// <inheritdoc />
        public async ValueTask<ICallHandle> CallAsync(IServiceProvider services, ImmutableArray<IChannelHandle> sources, ImmutableArray<IChannelHandle> outputs, CancellationToken cancellationToken)
        {
            var sourcesImpl = ImmutableArray.CreateBuilder<ChannelImpl>(sources.Length);
            foreach (var i in sources)
                sourcesImpl.Add((ChannelImpl)i);

            var outputsImpl = ImmutableArray.CreateBuilder<ChannelImpl>(outputs.Length);
            foreach (var i in outputs)
                outputsImpl.Add((ChannelImpl)i);

            var context = new CallImpl(_pipeline, services, Context, sourcesImpl.MoveToImmutable(), outputsImpl.MoveToImmutable());
            await context.StartAsync(cancellationToken);
            return context;
        }

    }

}
