using System;

namespace Gip.Core
{

    [Flags]
    public enum GipPadLinkFlags
    {

        CheckCaps = 1,
        CheckTemplateCaps = 2,
        CheckHierarchy = 4,
        Full = CheckCaps | CheckTemplateCaps | CheckHierarchy,

    }

}