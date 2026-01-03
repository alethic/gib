using System;

using Gip.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Gip.Hosting.AspNetCore
{

    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddAspNetCorePipeline(this IServiceCollection services, Uri? baseUri = null)
        {
            services.AddOptions<AspNetCorePipelineOptions>().Configure(o => o.BaseUri = baseUri);
            services.TryAddSingleton<IPipelineContext, AspNetCorePipeline>();
            return services;
        }

    }

}
