using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Gip.Core
{

    /// <summary>
    /// Describes a set of <see cref="GipCap"/> instances.
    /// </summary>
    public sealed class GipCapList : IReadOnlyList<GipCap>
    {

        /// <summary>
        /// Set of caps that accepts nothing.
        /// </summary>
        public static readonly GipCapList Empty = new GipCapList([]);

        /// <summary>
        /// Set of caps with a single cap accepting anything.
        /// </summary>
        public static readonly GipCapList Any = new GipCapList([new GipCap(null, GipConstraintList.Empty, [])]);

        /// <summary>
        /// Returns <c>true</c> if the two <see cref="GipCapList"/>s can intersect.
        /// </summary>
        /// <param name="caps1"></param>
        /// <param name="caps2"></param>
        /// <returns></returns>
        public static bool CanIntersect(GipCapList caps1, GipCapList caps2)
        {
            // caps are exactly the same reference
            if (caps1 == caps2)
                return true;

            var len1 = caps1.Count;
            var len2 = caps2.Count;

            // empty caps on either side
            if (caps1.Count == 0 || caps2.Count == 0)
                return false;

            // send caps contains an any entry (no type restriction)
            if (caps1.Any(i => i.Type == null && i.Features.Length == 0))
                return true;

            // sink caps contains an any entry (no type restriction)
            if (caps2.Any(i => i.Type == null && i.Features.Length == 0))
                return true;

            for (var i = 0; i < len1 + len2 - 1; i++)
            {
                // superset index goes from 0 to superset->structs->len-1 
                var j = Math.Min(i, len1 - 1);

                // subset index stays 0 until i reaches superset->structs->len, then it
                // counts up from 1 to subset->structs->len - 1
                var k = (i > j) ? (i - j) : 0;  /* MAX (0, i - j) */

                // now run the diagonal line, end condition is the left or bottom border
                while (k < len2)
                {
                    var cap1 = caps1[j];
                    var cap2 = caps2[k];

                    // features must all be equals, and compared caps must intersect
                    if (GipCapFeature.Equals(caps1[j].Features, caps2[k].Features) && GipCap.CanIntersect(cap1, cap2))
                        return true;

                    /* move down left */
                    k++;
                    if (j == 0)
                        break; /* so we don't roll back to G_MAXUINT */

                    j--;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a new <see cref="GipCapList"/> that contains all the formats that are common
        /// to both <paramref name="caps1"/> and <paramref name="caps2"/>, the order is defined by the <see cref="GipCapIntersectMode"/>
        /// used.
        /// </summary>
        /// <param name="caps1"></param>
        /// <param name="caps2"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static GipCapList Intersect(GipCapList caps1, GipCapList caps2, GipCapIntersectMode mode)
        {
            // caps are exactly the same pointers, just copy one caps
            if (ReferenceEquals(caps1, caps2))
                return caps1;

            // empty caps on either side
            if (caps1.Count == 0 || caps2.Count == 0)
                return Empty;

            // first caps contains an any entry (no type restriction)
            if (caps1.Any(i => i.Type == null && i.Features.Length == 0))
                return caps2;

            // second caps contains an any entry (no type restriction)
            if (caps2.Any(i => i.Type == null && i.Features.Length == 0))
                return caps1;

            return mode switch
            {
                GipCapIntersectMode.First => IntersectFirst(caps1, caps2),
                _ => IntersectZigZag(caps1, caps2),
            };
        }

        /// <summary>
        /// Creates a new <see cref="GipCapList"/> that contains all the formats that are common to both <paramref name="caps1"/> and <paramref name="caps2"/>.
        /// /// </summary>
        /// <param name="caps1"></param>
        /// <param name="caps2"></param>
        /// <returns></returns>
        static GipCapList IntersectFirst(GipCapList caps1, GipCapList caps2)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new <see cref="GipCapList"/> that contains all the formats that are common to both <paramref name="caps1"/> and <paramref name="caps2"/>.
        /// /// </summary>
        /// <param name="caps1"></param>
        /// <param name="caps2"></param>
        /// <returns></returns>
        static GipCapList IntersectZigZag(GipCapList caps1, GipCapList caps2)
        {
            var dest = new List<GipCap>();

            // Run zigzag on top line then right line, this preserves the caps order
            // much better than a simple loop.
            // 
            // This algorithm zigzags over the caps structures as demonstrated in
            // the following matrix:
            // 
            //          caps1
            //       +-------------
            //       | 1  2  4  7
            // caps2 | 3  5  8 10
            //       | 6  9 11 12
            // 
            // First we iterate over the caps1 structures (top line) intersecting
            // the structures diagonally down, then we iterate over the caps2
            // structures.
            for (var i = 0; i < caps1.Count + caps2.Count - 1; i++)
            {
                // superset index goes from 0 to superset->structs->len-1 
                var j = Math.Min(i, caps1.Count - 1);

                // subset index stays 0 until i reaches superset->structs->len, then it
                // counts up from 1 to subset->structs->len - 1
                var k = (i > j) ? (i - j) : 0;  /* MAX (0, i - j) */

                /* now run the diagonal line, end condition is the left or bottom
                 * border */
                while (k < caps2.Count)
                {
                    if (caps1[j].Type == caps2[k].Type && GipCapFeature.Equals(caps1[j].Features, caps2[k].Features))
                    {
                        var iconstraints = GipConstraint.Intersect(caps1[j].Constraints, caps2[k].Constraints);
                        if (iconstraints.Count > 0)
                        {
                            if (caps1[j].Features.Length == 0)
                                dest.Add(new GipCap(caps1[j].Type, iconstraints, caps2[k].Features));
                            else
                                dest.Add(new GipCap(caps1[j].Type, iconstraints, caps1[j].Features));
                        }
                    }

                    /* move down left */
                    k++;
                    if (j == 0)
                        break; /* so we don't roll back to Int32.Max */

                    j--;
                }
            }

            return new GipCapList(dest);
        }

        readonly GipCap[] list;

        /// <summary>
        /// Initializes a new instance from another underling list.
        /// </summary>
        /// <param name="caps"></param>
        public GipCapList(GipCap[] caps)
        {
            Array.Copy(caps, list = new GipCap[caps.Length], caps.Length);
        }

        /// <summary>
        /// Initializes a new instance from another underling list.
        /// </summary>
        /// <param name="caps"></param>
        public GipCapList(IReadOnlyList<GipCap> caps)
        {
            list = new GipCap[caps.Count];
            for (int i = 0; i < caps.Count; i++)
                list[i] = caps[i];
        }

        /// <inheritdoc />
        public GipCap this[int index] => ((IReadOnlyList<GipCap>)list)[index];

        /// <inheritdoc />
        public int Count => list.Length;

        /// <inheritdoc />
        public IEnumerator<GipCap> GetEnumerator() => ((IEnumerable<GipCap>)list).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

        /// <summary>
        /// Fixed <see cref="GipCap"/> describe exactly one format, that is, they have exactly
        /// one type with one set of constraints, and each field in the constraint describes a fixed type.
        /// </summary>
        /// <returns></returns>
        public bool IsFixed()
        {
            if (Count != 1)
                return false;

            // check that each of the constraints is fixed
            foreach (var cap in list)
                if (cap.GetIsFixed() == false)
                    return false;

            return true;
        }

    }

}
