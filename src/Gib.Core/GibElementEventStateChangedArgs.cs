namespace Gib.Core
{

    public record class GibElementEventStateChangedArgs(GibElement Element, GibElementState OldState, GibElementState NewState) : GibElementEventArgs(Element);

}
