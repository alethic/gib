namespace Gib.Core
{

    public record class GibElementEventPadArgs(GibElement Element, GibPad Pad, GibElementPadEventType Type) : GibElementEventArgs(Element);

}
