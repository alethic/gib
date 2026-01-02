using System.Threading;
using System.Threading.Tasks;

using Gib.Base.IO;
using Gib.Core.Elements;

namespace Gib.Console
{

    public partial class TestHostElement : ElementBase
    {

        public TestHostElement(IElementContext context) :
            base(context)
        {

        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var elementLoader = RegisterElement(new CsPipeElementLoader(Context));

            await Join(
                elementLoader.FileReader, 
                elementLoader.TextReader, 
                elementLoader.CsPipeProj, 
                OnElementsLoaded,
                cancellationToken);
        }

        async Task OnElementsLoaded(ElementReference fileReaderRef, ElementReference textReaderRef, ElementReference csPipeProjRef)
        {
            var fileReader = GetElement<IFileReader>(fileReaderRef.Uri);
            fileReader.Source = ValueOf(new FilePath("projectfile.cspipe"));

            var textReader = GetElement<ITextReader>(textReaderRef.Uri);
            textReader.Bytes = fileReader.Output;

            var csPipeProj = GetElement<ICsPipeProj>(csPipeProjRef.Uri);
            csPipeProj.WorkingDirectory = ValueOf("projectdir");
            csPipeProj.Code = textReader.Chars;
        }

    }

}
