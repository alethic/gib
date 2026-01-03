using Microsoft.Extensions.DependencyInjection;

namespace Gip.Hosting.AspNetCore
{

    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddGipClients(this IServiceCollection services)
        {
            //services.AddSingleton<IClientFactory, ClientFactory>();
            //services.AddSingleton<IClientProtocol, HttpClientProtocol>();
            services.AddHttpClient();
            return services;
        }

    }

}
