using Gib.Hosting.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Gib.Hosting.Fetchers.File
{

    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddGibFileFetcher(this IServiceCollection services)
        {
            services.TryAddSingleton<IFetcher, FileFetcher>();
            return services;
        }

    }

}
