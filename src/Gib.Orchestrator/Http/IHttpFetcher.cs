using System;

using Gib.Core.Elements;

namespace Gib.Orchestrator.Http
{

    public interface IHttpFetcher : IElementProxy
    {

        IValueBinding<Uri> HttpUri { get; set; }

        IValueBinding<string> LocalPath { get; }

    }

}
