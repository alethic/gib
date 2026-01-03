using System;

namespace Gip.Abstractions
{

    public interface IRemoteFunctionHandle : IFunctionHandle
    {

        Uri Uri { get; }

    }

}
