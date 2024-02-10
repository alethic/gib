using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Gip.Core
{

    /// <summary>
    /// Describes a set of <see cref="GipPad"/> instances associated with a <see cref="GipElement"/>.
    /// </summary>
    public sealed class GipPadCollection : IReadOnlyCollection<GipPad>
    {

        readonly GipElement element;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public GipPadCollection(GipElement element)
        {
            this.element = element;
        }

        /// <inheritdoc />
        public int Count
        {
            get { using (element.Lock()) { return element.pads.Count; } }
        }

        /// <inheritdoc />
        public IEnumerator<GipPad> GetEnumerator()
        {
            using (element.Lock())
                return element.pads.ToList().GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }

}
