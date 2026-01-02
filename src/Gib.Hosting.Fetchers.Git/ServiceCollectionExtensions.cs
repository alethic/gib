using Gib.Hosting.Abstractions;

using Microsoft.Extensions.DependencyInjection;

namespace Gib.Hosting.Fetchers.Git
{

    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddGibGitFetcher(this IServiceCollection services)
        {
            services.AddSingleton<IFetcher, GitFetcher>();
            return services;
        }

    }

}
