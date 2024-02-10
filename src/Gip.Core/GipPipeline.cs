namespace Gip.Core
{

    public class GibPipelineFactory : GipElementFactory
    {

        public static GibPipelineFactory Default = new GibPipelineFactory();

        /// <inheritdoc />
        public override GipElement Create()
        {
            return new GipPipeline(this);
        }

    }

    /// <summary>
    /// Top-level container for a Gib pipeline.
    /// </summary>
    public sealed class GipPipeline : GipBin
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        internal GipPipeline(GipElementFactory factory) :
            base(factory)
        {

        }

        /// <inheritdoc />
        protected override bool CanBeParentOf(GipObject obj)
        {
            // the pipeline can only be a parent of elements (no pads)
            return obj is GipElement;
        }

        /// <inheritdoc />
        protected override void SetParent(GipObject? parent)
        {
            if (parent is not null)
                throw new GipException($"Parent of {GetType().Name} must be empty.");

            base.SetParent(parent);
        }

    }

}
