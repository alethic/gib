using System;

using Gip.Abstractions;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Options;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Gip.Hosting.AspNetCore
{

    public class AspNetCorePipelineContext : DefaultPipelineContext
    {

        readonly IOptions<AspNetCorePipelineOptions> _options;
        readonly IServer _server;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="server"></param>
        public AspNetCorePipelineContext(IOptions<AspNetCorePipelineOptions> options, IServer server)
        {
            _options = options;
            _server = server;
        }

        /// <inheritdoc />
        public override FunctionReference GetFunctionReference(IFunctionHandle function)
        {
            return new FunctionReference(new Uri(GetAbsoluteUri("f/"), function.Id.ToString()));
        }

        /// <inheritdoc />
        public override ChannelReference GetChannelReference(IChannelHandle channel)
        {
            return new ChannelReference(new Uri(GetAbsoluteUri("c/"), channel.Id.ToString()));
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
