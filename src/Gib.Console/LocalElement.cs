using System;
using System.Threading;
using System.Threading.Tasks;

using Gib.Core.Elements;

namespace Gib.Console
{

    class LocalElement : ElementBase, IElementWithProxy<ILocalElement>
    {

        public LocalElement(IElementContext context) : base(context)
        {

        }

        public IValueConsumer<bool> DoThing { get; set; }

        public IValueProducer<bool> DidThing { get; set; }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }

}
