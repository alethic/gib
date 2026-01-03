using System;
using System.Linq;

using Gip.Abstractions;
using Gip.Abstractions.Clients;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Gip.Hosting.AspNetCore
{

    /// <summary>
    /// Implementation of <see cref="IPipelineContext"/> that supports functions and channels being made available over ASP.NET Core.
    /// </summary>
    public class AspNetCorePipeline : Pipeline
    {

        readonly IOptions<AspNetCorePipelineOptions> _options;
        readonly IClientFactory _clients;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="services"></param>
        /// <param name="clients"></param>
        public AspNetCorePipeline(IOptions<AspNetCorePipelineOptions> options, IServiceProvider services, IClientFactory clients) :
            base(services, clients)
        {
            _options = options;
            _clients = clients;
        }

        /// <inheritdoc />
        public override Uri GetLocalFunctionUri(Guid functionId)
        {
            return new Uri(GetAbsoluteUri("f/"), functionId.ToString());
        }

        /// <inheritdoc />
        public override Uri GetLocalChannelUri(Guid channelId)
        {
            return new Uri(GetAbsoluteUri("c/"), channelId.ToString());
        }

        /// <inheritdoc />
        public override bool TryGetLocalFunctionId(Uri uri, out Guid functionId)
        {
            var l = uri.Segments.LastOrDefault();
            if (Guid.TryParse(l, out var g) && GetLocalFunctionUri(g) == uri)
            {
                functionId = g;
                return true;
            }

            functionId = Guid.Empty;
            return false;
        }

        /// <inheritdoc />
        public override bool TryGetLocalChannelId(Uri uri, out Guid channelId)
        {
            var l = uri.Segments.LastOrDefault();
            if (Guid.TryParse(l, out var g) && GetLocalChannelUri(g) == uri)
            {
                channelId = g;
                return true;
            }

            channelId = Guid.Empty;
            return false;
        }

        /// <summary>
        /// Gets a the absolute URI of a relative path.
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        Uri GetAbsoluteUri(string relativePath) => new Uri(_options.Value.BaseUri ?? GetDefaultBaseUri(), relativePath);

        /// <summary>
        /// Attempts to get the default base URI.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        Uri GetDefaultBaseUri()
        {
            if (ServiceProvider.GetService<IServer>() is { } server)
                if (server.Features.Get<IServerAddressesFeature>() is { } addresses)
                    foreach (var address in addresses.Addresses)
                        if (string.IsNullOrEmpty(address) == false)
                            return new Uri(new Uri(EnsureEndsWithSlash(address)), "gip/");

            throw new InvalidOperationException("No configured base URI.");
        }

        /// <summary>
        /// Ensures the given value ends with a forward slash.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        static string EnsureEndsWithSlash(string v) => v.EndsWith('/') ? v : v + "/";

    }

}
