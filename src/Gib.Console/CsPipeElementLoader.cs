using System;
using System.Threading;
using System.Threading.Tasks;

using Gib.Core.Elements;

namespace Gib.Console
{

    interface ICsPipeLoader : IElementProxy
    {

        IValueBinding<ElementReference> FileReader { get; }

        IValueBinding<ElementReference> TextReader { get; }

        IValueBinding<ElementReference> CsPipeProj { get; }

    }

    class CsPipeElementLoader : ElementBase, IElementWithProxy<ICsPipeLoader>
    {

        public CsPipeElementLoader(IElementContext context) : base(context)
        {

        }

        public IValueProducer<ElementReference> FileReader { get; set; }

        public IValueProducer<ElementReference> TextReader { get; set; }

        public IValueProducer<ElementReference> CsPipeProj { get; set; }

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var fileReaderPackage = CreateElement<INuGetLoader>(new Uri("dotnet:builtin:NuGetLoader"));
            fileReaderPackage.PackageId = ValueOf("Gib.Base");
            fileReaderPackage.Version = ValueOf("1.2.3");

            var textReaderPackage = CreateElement<INuGetLoader>(new Uri("dotnet:builtin:NuGetLoader"));
            textReaderPackage.PackageId = ValueOf("Gib.Base");
            textReaderPackage.Version = ValueOf("1.2.3");

            var csPipeProjPackage = CreateElement<INuGetLoader>(new Uri("dotnet:builtin:NuGetLoader"));
            csPipeProjPackage.PackageId = ValueOf("Gib.Base");
            csPipeProjPackage.Version = ValueOf("1.2.3");

            await FileReader.BindAsync(fileReaderPackage.ElementReference);
            await TextReader.BindAsync(textReaderPackage.ElementReference);
            await CsPipeProj.BindAsync(csPipeProjPackage.ElementReference);
        }

    }

}
