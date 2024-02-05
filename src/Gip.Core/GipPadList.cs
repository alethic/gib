using System.Collections.Generic;

namespace Gip.Core
{

    /// <summary>
    /// Describes a set of <see cref="GipPad"/> instances.
    /// </summary>
    public sealed class GipPadList : List<GipPad>
    {

        /// <summary>
        /// Empty pad list.
        /// </summary>
        public static readonly GipPadList Empty = new GipPadList();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public GipPadList()
        {

        }

    }

}
