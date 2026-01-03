using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;

using Microsoft.Extensions.DependencyInjection;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Gip.Hosting
{

    /// <summary>
    /// Holds a reference to a registered service in the service container.
    /// </summary>
    class FunctionImpl : ILocalFunctionHandle
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
        public async ValueTask<ILocalCallHandle> CallAsync(ImmutableArray<IReadableChannelHandle?> sources, ImmutableArray<IWritableChannelHandle?> outputs, CancellationToken cancellationToken)
        {
            if (sources.Length != Schema.Sources.Length)
                throw new ArgumentException("Function call does not contain the expected number of sources.", nameof(sources));
            if (outputs.Length != Schema.Outputs.Length)
                throw new ArgumentException("Function call does not contain the expected number of outputs.", nameof(outputs));

            // copy the sources and fill in the missing channels with local channels
            var s = ImmutableArray.CreateBuilder<IReadableChannelHandle>(Schema.Sources.Length);
            for (int i = 0; i < Schema.Sources.Length; i++)
                s.Add(sources[i] ?? _pipeline.CreateChannel(Schema.Sources[i]));

            // copy the outputs and fill in the missing channels with local channels
            var o = ImmutableArray.CreateBuilder<IWritableChannelHandle>(Schema.Outputs.Length);
            for (int i = 0; i < Schema.Outputs.Length; i++)
                o.Add(outputs[i] ?? _pipeline.CreateChannel(Schema.Outputs[i]));

            var context = new CallImpl(_pipeline, _pipeline.ServiceProvider.CreateAsyncScope(), Context, s.MoveToImmutable(), o.MoveToImmutable());
            await context.StartAsync(cancellationToken);
            return context;
        }

        /// <inheritdoc />
        async ValueTask<ICallHandle> IFunctionHandle.CallAsync(ImmutableArray<IReadableChannelHandle?> sources, ImmutableArray<IWritableChannelHandle?> outputs, CancellationToken cancellationToken)
        {
            return await CallAsync(sources, outputs, cancellationToken);
        }

    }

}
