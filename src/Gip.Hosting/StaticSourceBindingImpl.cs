using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Gip.Abstractions;

namespace Gip.Hosting
{

    class StaticSourceBindingImpl : SourceBinding
    {

        readonly CallContextImpl _context;
        readonly ChannelSchema _schema;
        readonly StaticSourceParameter _parameter;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="schema"></param>
        /// <param name="parameter"></param>
        public StaticSourceBindingImpl(CallContextImpl context, ChannelSchema schema, StaticSourceParameter parameter)
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

            return _parameter.Signals.Cast<T>().ToAsyncEnumerable();
        }
    }

}
