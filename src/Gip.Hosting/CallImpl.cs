using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;

namespace Gip.Hosting
{

    /// <summary>
    /// Represents an outstanding function call.
    /// </summary>
    class CallImpl : ICallContext, ICallHandle
    {

        readonly Pipeline _pipeline;
        readonly IServiceProvider _services;
        readonly IFunctionContext _function;
        readonly ImmutableArray<ChannelImpl> _sources;
        readonly ImmutableArray<ChannelImpl> _outputs;

        CancellationTokenSource? _stop;
        Task? _task;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="services"></param>
        /// <param name="function"></param>
        /// <param name="sources"></param>
        /// <param name="outputs"></param>
        /// <exception cref="ArgumentException"></exception>
        public CallImpl(Pipeline pipeline, IServiceProvider services, IFunctionContext function, ImmutableArray<ChannelImpl> sources, ImmutableArray<ChannelImpl> outputs)
        {
            if (sources.Length != function.Schema.Sources.Length)
                throw new ArgumentException("Function does not contain the expected number of sources.", nameof(sources));
            if (outputs.Length != function.Schema.Outputs.Length)
                throw new ArgumentException("Function does not contain the expected number of outputs.", nameof(outputs));

            _pipeline = pipeline;
            _services = services;
            _function = function;
            _sources = sources;
            _outputs = outputs;
        }

        /// <inheritdoc />
        public Pipeline Pipeline => _pipeline;

        /// <inheritdoc />
        public IServiceProvider Services => _services;

        /// <inheritdoc />
        public ImmutableArray<ChannelImpl> Sources => _sources;

        /// <inheritdoc />
        public ImmutableArray<ChannelImpl> Outputs => _outputs;

        ImmutableArray<IChannelHandle> ICallContext.Sources => _sources.CastArray<IChannelHandle>();

        ImmutableArray<IChannelHandle> ICallContext.Outputs => _outputs.CastArray<IChannelHandle>();

        /// <inheritdoc />
        ImmutableArray<IChannelHandle> ICallHandle.Outputs => _outputs.CastArray<IChannelHandle>();

        /// <inheritdoc />
        IPipelineContext ICallContext.Pipeline => Pipeline;

        /// <summary>
        /// Runs the call. This method returns when the call is complete.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ValueTask StartAsync(CancellationToken cancellationToken)
        {
            if (_task != null || _stop != null)
                throw new InvalidOperationException();

            _stop = new CancellationTokenSource();
            _task = _function.CallAsync(this, _stop.Token);
            return default;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                _stop?.Cancel();

                if (_task is not null)
                {
                    if (_task.IsCompleted)
                        return;

                    _task?.GetAwaiter().GetResult();
                }
            }
            catch
            {
                // ignore
            }
            finally
            {
                _stop = null;
                _task = null;
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            try
            {
                _stop?.Cancel();

                if (_task is not null)
                {
                    if (_task.IsCompleted)
                        return;

                    await _task;
                }
            }
            catch
            {
                // ignore
            }
            finally
            {
                _stop = null;
                _task = null;
            }
        }

    }

}
