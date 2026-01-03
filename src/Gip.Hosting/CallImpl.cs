using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;

using Microsoft.Extensions.DependencyInjection;

namespace Gip.Hosting
{

    /// <summary>
    /// Represents an outstanding function call.
    /// </summary>
    class CallImpl : ICallContext, ILocalCallHandle
    {

        readonly Pipeline _pipeline;
        readonly AsyncServiceScope _services;
        readonly IFunctionContext _function;
        readonly ImmutableArray<IChannelHandle> _sources;
        readonly ImmutableArray<IChannelHandle> _outputs;

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
        public CallImpl(Pipeline pipeline, AsyncServiceScope services, IFunctionContext function, ImmutableArray<IChannelHandle> sources, ImmutableArray<IChannelHandle> outputs)
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
        public IServiceProvider Services => _services.ServiceProvider;

        /// <inheritdoc />
        public ImmutableArray<IChannelHandle> Sources => _sources;

        /// <inheritdoc />
        public ImmutableArray<IChannelHandle> Outputs => _outputs;

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
            _task = _function.CallAsync(this, _stop.Token).ContinueWith(t => { GC.KeepAlive(this); }); // we keep the CallImpl alive until task completes
            return default;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);

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

            _services.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);

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

            await _services.DisposeAsync();
        }

        /// <summary>
        /// Finalizes the instance.
        /// </summary>
        ~CallImpl()
        {
            Dispose();
        }

    }

}
