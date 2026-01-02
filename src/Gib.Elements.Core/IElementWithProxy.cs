namespace Gib.Core.Elements
{

    public interface IElementWithProxy<TProxy> : IElement
        where TProxy : IElementProxy
    {



    }

}