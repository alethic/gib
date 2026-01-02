using System.Collections.Generic;
using System.Threading;

using Gip.Abstractions;

namespace Gip.Hosting
{

    class RemoteSourceBinding : SourceBinding
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
        public RemoteSourceBinding(ICallContext context, ChannelSchema schema, RemoteSourceParameter parameter)
        {
            _context = context;
            _schema = schema;
            _parameter = parameter;
        }

        /// <inheritdoc />
        public override ChannelSchema Schema => _schema;

        /// <inheritdoc />
        public override IAsyncEnumerable<T> OpenAsync<T>(CancellationToken cancellationToken) => _context.OpenRemoteAsync<T>(_parameter.Uri, cancellationToken);

    }

}
