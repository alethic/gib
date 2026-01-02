using System;
using System.Collections.Generic;
using System.Threading;

using Gip.Abstractions;

namespace Gip.Hosting
{

    class RemoteSourceBindingImpl : SourceBinding
    {

        readonly ICallContext _context;
        readonly ChannelSchema _schema;
        readonly RemoteSourceParameter _parameter;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="schema"></param>
        /// <param name="parameter"></param>
        public RemoteSourceBindingImpl(ICallContext context, ChannelSchema schema, RemoteSourceParameter parameter)
        {
            _context = context;
            _schema = schema;
            _parameter = parameter;
        }

        /// <inheritdoc />
        public override ChannelSchema Schema => _schema;

        /// <inheritdoc />
        public override IAsyncEnumerable<T> OpenAsync<T>(CancellationToken cancellationToken)
        {
            if (typeof(T) != Schema.Signal)
                throw new ArgumentException($"Type {typeof(T)} is not compatible with channel schema type {Schema.Signal}.");

            return _context.OpenRemoteAsync<T>(_parameter.Uri, cancellationToken);
        }
    }

}
