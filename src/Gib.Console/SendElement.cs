using Gib.Core;

namespace Gib.Console
{

    public class SendElement : GibElement
    {

        protected override bool TryChangeState(GibElementState state)
        {
            return true;
        }

    }

}
