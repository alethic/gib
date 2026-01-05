using System;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Core;

namespace Gib.Core
{

    /// <summary>
    /// <see cref="IFunctionContext"/> implementation that exposes a Gib element.
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    public class ElementFunctionContext<TElement> : FunctionContextBase
        where TElement : IElement
    {

        public override FunctionSchema Schema => throw new NotImplementedException();

        public override Task CallAsync(ICallContext call, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }

}
