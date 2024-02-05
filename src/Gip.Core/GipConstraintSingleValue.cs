namespace Gip.Core
{

    /// <summary>
    /// Describes a cap feature that possess a single static value that must match.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GipConstraintSingleValue<T> : GipConstraintValue
    {

        readonly T value;

        /// <summary>
        /// Initializes a new intstance.
        /// </summary>
        /// <param name="value"></param>
        public GipConstraintSingleValue(T value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets the single static value that must match.
        /// </summary>
        public T Value => value;

        public override bool IsFixed => throw new System.NotImplementedException();

        public override bool CanIntersect(object? other)
        {
            throw new System.NotImplementedException();
        }

        public override bool TryIntersect(object? other, out object? result)
        {
            throw new System.NotImplementedException();
        }

    }

}
