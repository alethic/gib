using System.Threading;
using System.Threading.Tasks;

using Gib.Core;
using Gib.Core.Elements;

namespace Gib.Console
{

    public class ExampleDslElement : ElementBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public ExampleDslElement(IElementContext context) :
            base(context)
        {

        }

        /// <summary>
        /// List of items to result in the creation of an element.
        /// </summary>
        [Property("code")]
        public required string Code { get; set; }

        /// <inheritdoc />
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // when code changes, we reload DSL, and run it to create child elements
        }

    }

}
