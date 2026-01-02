using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Abstractions.Clients;

using Microsoft.Extensions.DependencyInjection;

namespace Gip.Hosting
{

    /// <summary>
    /// Represents an outstanding function call.
    /// </summary>
    class CallContextImpl : ICallContext, ICallHandle
    {

        readonly DefaultPipelineContext _host;
        readonly IServiceProvider _services;
        readonly IFunctionContext _function;
        readonly ImmutableArray<SourceBinding> _sources;
        readonly ImmutableArray<OutputBinding> _outputs;
        readonly ImmutableArray<OutputParameter> _outputParameters;

        CancellationTokenSource? _stop;
        Task? _task;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="services"></param>
        /// <param name="function"></param>
        /// <param name="parameters"></param>
        /// <exception cref="ArgumentException"></exception>
        public CallContextImpl(DefaultPipelineContext host, IServiceProvider services, IFunctionContext function, ImmutableArray<SourceParameter> parameters)
        {
            _host = host;
            _services = services;
            _function = function;

            // check that passed parameter length equals the function parameter length
            if (parameters.Length != _function.Schema.Sources.Length)
                throw new ArgumentException("", nameof(parameters));

            var sourceBindings = ImmutableArray.CreateBuilder<SourceBinding>(_function.Schema.Sources.Length);
            for (int i = 0; i < _function.Schema.Sources.Length; i++)
            {
                sourceBindings.Add(parameters[i] switch
                {
                    StaticSourceParameter staticParam => new StaticSourceBindingImpl(this, _function.Schema.Sources[i], staticParam),
                    RemoteSourceParameter remoteParam => new RemoteSourceBindingImpl(this, _function.Schema.Sources[i], remoteParam),
                    _ => throw new ArgumentException("", nameof(parameters)),
                });
            }
            _sources = sourceBindings.ToImmutable();

            var outputBindings = ImmutableArray.CreateBuilder<OutputBinding>(_function.Schema.Outputs.Length);
            var outputParameters = ImmutableArray.CreateBuilder<OutputParameter>(_function.Schema.Outputs.Length);
            for (int i = 0; i < _function.Schema.Outputs.Length; i++)
            {
                var channel = _host.CreateChannel(_function.Schema.Outputs[i]);
                outputBindings.Add(new OutputBindingImpl(channel.Schema, channel));
                outputParameters.Add(new OutputParameter(channel.Id, channel.Schema));
            }

            _outputs = outputBindings.ToImmutable();
            _outputParameters = outputParameters.ToImmutable();
        }

        /// <inheritdoc />
        public IPipelineContext Host => _host;

        /// <inheritdoc />
        public IServiceProvider Services => _services;

        /// <inheritdoc />
        public ImmutableArray<SourceBinding> Sources => _sources;

        /// <inheritdoc />
        public ImmutableArray<OutputBinding> Outputs => _outputs;

        /// <inheritdoc />
        ImmutableArray<OutputParameter> ICallHandle.Outputs => _outputParameters;

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
        async IAsyncEnumerable<T> ICallContext.OpenRemoteAsync<T>(Uri channelUri, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var i in _services.GetRequiredService<IClientFactory>().GetChannel<T>(channelUri).WithCancellation(cancellationToken))
                yield return i;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                _stop?.Cancel();
                _task?.GetAwaiter().GetResult();
            }
            catch
            {
                // ignore
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            try
            {
                _stop?.Cancel();
                if (_task is not null)
                    await _task;
            }
            catch
            {
                // ignore
            }
        }

    }

}
