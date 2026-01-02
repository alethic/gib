using Gib.Core;

namespace Gib.DotNet
{

    public class LibraryRefElement : ElementBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public LibraryRefElement(IElementContext context) : 
            base(context)
        {

        }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }

}
