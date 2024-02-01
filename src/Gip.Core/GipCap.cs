using System;

namespace Gip.Core
{

    /// <summary>
    /// Describes a single advertised capability.
    /// </summary>
    /// <param name="Type"></param>
    /// <param name="Constraints"></param>
    /// <param name="Features"></param>
    public readonly record struct GipCap(Type? Type, GipConstraintList Constraints, GipCapFeatureList Features)
    {

        /// <summary>
        /// Fixed <see cref="GipCap"/> describe exactly one format, that is, they have exactly
        /// one type with one set of constraints, and each field in the constraint describes a fixed type.
        /// </summary>
        /// <returns></returns>
        public readonly bool IsFixed => GetIsFixed();

        /// <summary>
        /// Implements the getter for <see cref="IsFixed"/>.
        /// </summary>
        /// <returns></returns>
        readonly bool GetIsFixed()
        {
            // a missing type indicates we can accept any type
            if (Type == null)
                return false;

            // all constraints must be fixed
            foreach (var constraint in Constraints)
                if (constraint.GetIsFixed() == false)
                    return false;

            return true;
        }

        /// <summary>
        /// Returns <c>true</c> if this cap can intersect with the cap given by <paramref name="other"/>.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public readonly bool CanIntersect(GipCap other)
        {
            if (Type != other.Type)
                return false;

            // check that all constraints of cap1 can intersect with the restrictions imposed by cap2
            return GipConstraint.CanIntersect(Constraints, other.Constraints);
        }

    }

}
