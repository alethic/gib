using Gib.Core;

namespace Gib.Console
{

    public class SinkElement : GibElement
    {

        protected override bool TryChangeState(GibElementState state)
        {
            return true;
        }

    }

}
