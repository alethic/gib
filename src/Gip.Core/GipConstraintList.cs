using System;
using System.Collections;
using System.Collections.Generic;

namespace Gip.Core
{

    /// <summary>
    /// Describes a set of <see cref="GipConstraint"/> instances.
    /// </summary>
    public sealed class GipConstraintList : IReadOnlyList<GipConstraint>
    {

        /// <summary>
        /// Empty set of constraints.
        /// </summary>
        public static readonly GipConstraintList Empty = new GipConstraintList([]);

        readonly GipConstraint[] list;

        /// <summary>
        /// Initializes a new instance from another underling list.
        /// </summary>
        /// <param name="caps"></param>
        public GipConstraintList(GipConstraint[] caps)
        {
            Array.Copy(caps, list = new GipConstraint[caps.Length], caps.Length);
        }

        /// <summary>
        /// Initializes a new instance from another underling list.
        /// </summary>
        /// <param name="caps"></param>
        public GipConstraintList(IReadOnlyList<GipConstraint> caps)
        {
            list = new GipConstraint[caps.Count];
            for (int i = 0; i < caps.Count; i++)
                list[i] = caps[i];
        }

        /// <inheritdoc />
        public GipConstraint this[int index] => ((IReadOnlyList<GipConstraint>)list)[index];

        /// <inheritdoc />
        public int Count => ((IReadOnlyCollection<GipConstraint>)list).Count;

        /// <inheritdoc />
        public IEnumerator<GipConstraint> GetEnumerator() => ((IEnumerable<GipConstraint>)list).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is GipConstraintList other && Equals(other);

        public bool Equals(GipConstraintList other) => Equals(this, other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = 12341;

            foreach (var item in this)
            {
                int h = EqualityComparer<GipConstraint>.Default.GetHashCode(item);
                if (h != 0)
                    hash = unchecked(hash * h);
            }

            return hash;
        }

    }

}
