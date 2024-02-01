using Gip.Core;

namespace Gip.Console
{

    public class SinkElement : GibElement
    {

        protected override bool TryChangeState(GibState state)
        {
            return true;
        }

    }

}
