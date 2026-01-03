using Gip.Abstractions.Clients;
using Gip.Core.Clients;
using Gip.Core.Clients.Http;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Gip.Hosting.AspNetCore
{

    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddGipClients(this IServiceCollection services)
        {
            services.TryAddSingleton<IClientFactory, ClientFactory>();
            services.TryAddSingleton<IClientProtocol, HttpClientProtocol>();
            services.AddHttpClient();
            return services;
        }

    }

}
