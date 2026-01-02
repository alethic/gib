using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Gip.Abstractions;

namespace Gip.Hosting
{

    class StaticSourceBinding : SourceBinding
    {

        readonly LocalCallContext _context;
        readonly ChannelSchema _schema;
        readonly StaticSourceParameter _parameter;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="schema"></param>
        /// <param name="parameter"></param>
        public StaticSourceBinding(LocalCallContext context, ChannelSchema schema, StaticSourceParameter parameter)
        {
            _context = context;
            _schema = schema;
            _parameter = parameter;
        }

        /// <inheritdoc />
        public override ChannelSchema Schema => _schema;

        /// <inheritdoc />
        public override IAsyncEnumerable<T> OpenAsync<T>(CancellationToken cancellationToken) => _parameter.Signals.Cast<T>().ToAsyncEnumerable();

    }

}
