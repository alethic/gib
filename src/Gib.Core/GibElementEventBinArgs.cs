namespace Gib.Core
{

    public record class GibElementEventBinArgs(GibElement Element, GibBin? OldBin, GibBin? NewBin) : GibElementEventArgs(Element);

}
