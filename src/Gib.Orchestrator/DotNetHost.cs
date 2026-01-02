namespace Gib.Orchestrator
{

    /// <summary>
    /// <see cref="IHost"/> implementation for the internal 'dotnet' host.
    /// </summary>
    class DotNetHost : IHost
    {

        readonly DotNetHostClient _client = new();

        /// <inheritdoc />
        public IHostClient Client => _client;

    }

}
