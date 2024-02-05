using System;
using System.Collections.Generic;

namespace Gip.Core
{

    /// <summary>
    /// Describes a named constraint of a <see cref="GipCap."/>;
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="Value"></param>
    public readonly record struct GipConstraint(string Name, object? Value)
    {

        /// <summary>
        /// Determines if the constraint value is fixed.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsFixedValue(object value)
        {
            return value is not GipConstraintValue v || v.IsFixed;
        }

        /// <summary>
        /// Returns <c>true</c> if the constraints given by <paramref name="constraints1"/> intersect with the constraints given by <paramref name="constraints2"/>.
        /// </summary>
        /// <param name="constraints1"></param>
        /// <param name="constraints2"></param>
        /// <returns></returns>
        public static bool CanIntersect(GipConstraintList constraints1, GipConstraintList constraints2)
        {
            foreach (var constraint in constraints1)
                if (CanIntersect(constraint, constraints2) == false)
                    return false;

            return true;
        }

        /// <summary>
        /// Returns <c>true</c> if the constraint given by <paramref name="constraint"/> intersect with the constraints given by <paramref name="constraints"/>.
        /// </summary>
        /// <param name="constraint"></param>
        /// <param name="constraints"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        static bool CanIntersect(GipConstraint constraint, GipConstraintList constraints)
        {
            // must intersect with all constraints
            foreach (var c in constraints)
                if (CanIntersect(constraint, c) == false)
                    return false;

            // we did
            return true;
        }

        /// <summary>
        /// Returns <c>true</c> if the constraint given by <paramref name="constraint1"/> intersects with the constraint given by <paramref name="constraint2"/>.
        /// </summary>
        /// <param name="constraint1"></param>
        /// <param name="constraint2"></param>
        /// <returns></returns>
        public static bool CanIntersect(GipConstraint constraint1, GipConstraint constraint2)
        {
            // constraint is for the same name, check if values intersect
            if (constraint1.Name == constraint2.Name)
                return CanIntersect(constraint1.Value, constraint2.Value);

            // differnet name, no requirement for match
            return true;
        }

        /// <summary>
        /// Returns <c>true</c> if the constraint value given by <paramref name="value1"/> intersects with the constraint value given by <paramref name="value2"/>.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        static bool CanIntersect(object? value1, object? value2)
        {
            // exact same object
            if (ReferenceEquals(value1, value2))
                return true;

            // one or the other is a special constraint value
            var v1 = value1 as GipConstraintValue;
            var v2 = value2 as GipConstraintValue;
            if (v1 != null && v2 != null)
                return GipConstraintValue.CanIntersect(v1, v2);
            if (v1 != null && v2 == null)
                return GipConstraintValue.CanIntersect(v1, value2);
            if (v1 == null && v2 != null)
                return GipConstraintValue.CanIntersect(value1, v2);

            // fallback to general object equality
            return Equals(value1, value2);
        }

        /// <summary>
        /// Intersects <paramref name="constraints1"/> and <paramref name="constraints2"/> and returns the intersection.
        /// </summary>
        /// <param name="constraints1"></param>
        /// <param name="constraints2"></param>
        /// <returns></returns>
        public static GipConstraintList Intersect(GipConstraintList constraints1, GipConstraintList constraints2)
        {
            var len1 = constraints1.Count;
            var len2 = constraints2.Count;

            // resulting structure will be at most the size of the smallest structure
            var dest = new List<GipConstraint>(Math.Min(len1, len2));

            // copy fields from constraints1 which we have not in constraints2 to target
            // intersect if we have the field in both
            for (var it1 = 0; it1 < len1; it1++)
            {
                var field1 = constraints1[it1];

                // track whether we found a matching constraint
                var seenother = false;

                for (var it2 = 0; it2 < len2; it2++)
                {
                    var field2 = constraints2[it2];

                    // find matching field
                    if (field1.Name == field2.Name)
                    {
                        // we found matching constraint
                        seenother = true;

                        // obtain the intersection for the value of the two constraints
                        if (GipConstraintValue.TryIntersect(field1.Value, field2.Value, out var value))
                        {
                            seenother = true;
                            dest.Add(new GipConstraint(field1.Name, value));
                        }
                    }
                }

                // field1 was only present in constraint1, copy it over
                if (seenother == false)
                    dest.Add(new GipConstraint(field1.Name, field1.Value));
            }

            // Now iterate over the 2nd struct and copy over everything which
            // isn't present in the 1st struct (we've already taken care of
            // values being present in both just above)
            for (var it2 = 0; it2 < len2; it2++)
            {
                var field2 = constraints2[it2];

                // track whether we found a matching constraint
                var seenother = false;

                for (var it1 = 0; it1 < len1; it1++)
                {
                    var field1 = constraints1[it1];
                    if (field1.Name == field2.Name)
                    {
                        seenother = true;
                        break;
                    }
                }

                if (seenother == false)
                    dest.Add(new GipConstraint(field2.Name, field2.Value));
            }

            // return resulting constraints
            return new GipConstraintList(dest);
        }

        /// <summary>
        /// Gets whether the constraint is a fixed constraint. That is, all values are scalar instances and not ranges.
        /// </summary>
        public bool IsFixed => GetIsFixed();

        /// <summary>
        /// Determines if the constraint is a fixed constraint. That is, all values are scalar instances and not ranges.
        /// </summary>
        /// <returns></returns>
        bool GetIsFixed()
        {
            return Value == null || IsFixedValue(Value);
        }

    }

}
