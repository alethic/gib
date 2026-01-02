namespace Gib.Orchestrator
{

    public interface IHost
    {

        IHostClient Client { get; }

        string Name { get; }

    }

}
