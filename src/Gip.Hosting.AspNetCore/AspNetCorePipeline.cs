using System;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Options;

namespace Gip.Hosting.AspNetCore
{

    /// <summary>
    /// Implementation of <see cref="IPipelineContext"/> that supports functions and channels being made available over ASP.NET Core.
    /// </summary>
    public class AspNetCorePipeline : Pipeline
    {

        readonly IOptions<AspNetCorePipelineOptions> _options;
        readonly IServer _server;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="services"></param>
        /// <param name="server"></param>
        public AspNetCorePipeline(IOptions<AspNetCorePipelineOptions> options, IServiceProvider services, IServer server) :
            base(services)
        {
            _options = options;
            _server = server;
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
            if (_server.Features.Get<IServerAddressesFeature>() is { } _addresses)
                foreach (var address in _addresses.Addresses)
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
