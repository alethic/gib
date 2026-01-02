using Gib.Containers.Library;
using Gib.Hosting.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Gib.Hosting.Handlers.Library
{

    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddGibLibraryHandler(this IServiceCollection services)
        {
            services.TryAddSingleton<IElementTypeHandler, LibraryElementTypeHandler>();
            services.TryAddSingleton<LibraryHost>();
            return services;
        }

    }

}
