using System;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Hosting.AspNetCore.Converters;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Gip.Hosting.AspNetCore.Sample
{

    public static class Program
    {

        static IFunctionHandle _testHandle;

        public static Task Main(string[] args) => global::Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureWebHostDefaults(b => b.Configure(ConfigureApplication))
            .ConfigureServices(ConfigureServices)
            .Build()
            .RunAsync();

        /// <summary>
        /// Configures the container services.
        /// </summary>
        /// <param name="services"></param>
        static void ConfigureServices(IServiceCollection services)
        {
            services.AddAspNetCorePipeline();
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="app"></param>
        static void ConfigureApplication(WebHostBuilderContext ctx, IApplicationBuilder app)
        {
            var pipeline = app.ApplicationServices.GetRequiredService<IPipelineContext>();
            _testHandle = pipeline.CreateFunction(new RootContext());
            Console.WriteLine(_testHandle.Id);

            app.UseForwardedHeaders();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapPipelineHost("gip");
            });
        }

    }

}
