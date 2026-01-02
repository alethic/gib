using Gip.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Gip.Hosting
{

    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddPipelineHost(this IServiceCollection services)
        {
            services.TryAddSingleton<IPipelineHost, LocalPipelineHost>();
            return services;
        }

    }

}
