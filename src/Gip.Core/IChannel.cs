using System.Collections.Generic;

namespace Gip.Core
{

    public interface IChannel<out TSignal> : IAsyncEnumerable<TSignal>
    {



    }

}
