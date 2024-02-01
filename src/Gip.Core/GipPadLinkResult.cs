namespace Gip.Core
{

    /// <summary>
    /// Return values from an attempt to link pads.
    /// </summary>
    public enum GipPadLinkResult
    {

        Ok,
        WrongHierarchy,
        WasLinked,
        WrongDirection,
        NoFormat,
        Refused,

    }

}
