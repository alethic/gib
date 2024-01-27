namespace Gib.Core
{

    public sealed class GibPipeline : GibBin
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public GibPipeline()
        {
        }

        /// <inheritdoc />
        protected internal override void SetBin(GibBin? bin)
        {
            throw new GibException($"{nameof(GibPipeline)} cannot be a member of a {nameof(Bin)}.");
        }

    }

}
