namespace Gip.Core
{

    /// <summary>
    /// Descsribes the value of a capability feature.
    /// </summary>
    public abstract class GipConstraintValue
    {

        /// <summary>
        /// Returns <c>true</c> if the constraint value given by <paramref name="value1"/> intersects with the object value given by <paramref name="value2"/>.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static bool CanIntersect(object? value1, object? value2)
        {
            // exact same object
            if (ReferenceEquals(value1, value2))
                return true;

            // one or the other is a special constraint value
            var v1 = value1 as GipConstraintValue;
            var v2 = value2 as GipConstraintValue;

            // both values are a constraint value
            if (v1 != null && v2 != null)
                return v1.CanIntersect(v2);

            // only value1 is a constraint value
            if (v1 != null && v2 == null)
                return v1.CanIntersect(value2);

            // only value2 is a constraint value
            if (v1 == null && v2 != null)
                return v2.CanIntersect(value1);

            // intersection depends on general object equality
            return Equals(value1, value2);
        }

        /// <summary>
        /// Computers the intersection of the two values and outputs the result.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static bool TryIntersect(object? value1, object? value2, out object? result)
        {
            // exact same object
            if (ReferenceEquals(value1, value2))
            {
                result = value1;
                return true;
            }

            // one or the other is a special constraint value
            var v1 = value1 as GipConstraintValue;
            var v2 = value2 as GipConstraintValue;

            // both values are a constraint value
            if (v1 != null && v2 != null)
                return v1.TryIntersect(v2, out result);

            // only value1 is a constraint value
            if (v1 != null && v2 == null)
                return v1.TryIntersect(value2, out result);

            // only value2 is a constraint value
            if (v1 == null && v2 != null)
                return v2.TryIntersect(value1, out result);

            // intersection depends on general object equality
            if (Equals(value1, value2))
            {
                result = value1;
                return true;
            }

            // by default they did not intersect
            result = null;
            return false;
        }

        /// <summary>
        /// Returns <c>true</c> if the constraint is a fixed constraint. That is, it cannot allow variable values.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsFixed { get; }

        /// <summary>
        /// Returns <c>true</c> if this value intersects with the specified other value.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract bool CanIntersect(object? other);

        /// <summary>
        /// Attempts to intersect this value with the other specified value.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public abstract bool TryIntersect(object? other, out object? result);

    }

}