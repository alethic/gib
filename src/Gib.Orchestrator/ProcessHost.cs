using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Gib.Orchestrator
{

    /// <summary>
    /// <see cref="IHost"/> implementation that is served by an external process.
    /// </summary>
    public class ProcessHost : IHost
    {

        /// <summary>
        /// Gets a randomized TCP port.
        /// </summary>
        /// <returns></returns>
        static int FreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        /// <summary>
        /// Starts a new host that lives at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ProcessHost Start(string path)
        {
            var port = FreeTcpPort();

            var proc = new Process();
            proc.StartInfo.FileName = path;
            proc.StartInfo.ArgumentList.Add($"https://localhost:{port}");

            if (proc.Start() == false)
                throw new InvalidOperationException();

            return new ProcessHost(proc);
        }

        readonly Process _process;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="path"></param>
        public ProcessHost(Process process)
        {
            _process = process;
        }

        /// <summary>
        /// Invoke to stop the host.
        /// </summary>
        public ValueTask StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }

}
