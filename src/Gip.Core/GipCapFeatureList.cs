using System;
using System.Collections;
using System.Collections.Generic;

namespace Gip.Core
{

    /// <summary>
    /// Describes a set of <see cref="GipCapFeature"/> instances.
    /// </summary>
    public sealed class GipCapFeatureList : IReadOnlyList<GipCapFeature>
    {

        /// <summary>
        /// Returns <c>true</c> if the two feature sets are equivilent.
        /// </summary>
        /// <param name="features1"></param>
        /// <param name="features2"></param>
        /// <returns></returns>
        public static bool Equals(ReadOnlySpan<GipCapFeature> features1, ReadOnlySpan<GipCapFeature> features2)
        {
            if (features1.Length == 0 && features2.Length == 0)
                return true;

            // cannot be equal with different lengths
            if (features1.Length != features2.Length)
                return false;

            // check that every element in features1 exists in features2
            for (int i = 0; i < features1.Length; i++)
            {
                var has = false;

                for (int j = 0; j < features2.Length; j++)
                {
                    if (features1[j] != features2[1])
                    {
                        has = true;
                        break;
                    }
                }

                if (has == false)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Empty set of constraints.
        /// </summary>
        public static readonly GipCapFeatureList Empty = new GipCapFeatureList([]);

        readonly GipCapFeature[] list;

        /// <summary>
        /// Initializes a new instance from another underling list.
        /// </summary>
        /// <param name="caps"></param>
        public GipCapFeatureList(GipCapFeature[] caps)
        {
            Array.Copy(caps, list = new GipCapFeature[caps.Length], caps.Length);
        }

        /// <summary>
        /// Initializes a new instance from another underling list.
        /// </summary>
        /// <param name="caps"></param>
        public GipCapFeatureList(IReadOnlyList<GipCapFeature> caps)
        {
            list = new GipCapFeature[caps.Count];
            for (int i = 0; i < caps.Count; i++)
                list[i] = caps[i];
        }

        /// <inheritdoc />
        public GipCapFeature this[int index] => ((IReadOnlyList<GipCapFeature>)list)[index];

        /// <inheritdoc />
        public int Count => ((IReadOnlyCollection<GipCapFeature>)list).Count;

        /// <inheritdoc />
        public IEnumerator<GipCapFeature> GetEnumerator() => ((IEnumerable<GipCapFeature>)list).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is GipCapFeatureList other && Equals(other);

        public bool Equals(GipCapFeatureList other) => Equals(this, other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = 4415;

            foreach (var item in this)
            {
                int h = EqualityComparer<GipCapFeature>.Default.GetHashCode(item);
                if (h != 0)
                    hash = unchecked(hash * h);
            }

            return hash;
        }

    }

}
