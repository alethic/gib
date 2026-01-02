using System;
using System.Collections.Immutable;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Core.Clients.Http.Json;
using Gip.Hosting.AspNetCore.Converters;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Gip.Hosting.AspNetCore
{

    public static class EndpointRouteBuilderExtensions
    {

        /// <summary>
        /// Non-generic interface for JSON channel writers.
        /// </summary>
        interface IJsonChannelWriter
        {

            ValueTask WriteChannelAsync(HttpContext context, IChannelHandle channel, CancellationToken cancellationToken);

        }

        /// <summary>
        /// Generic version of writer. Allows typed invocation of OpenAsync.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class JsonChannelWriter<T> : IJsonChannelWriter
        {

            public async ValueTask WriteChannelAsync(HttpContext context, IChannelHandle channel, CancellationToken cancellationToken)
            {
                await foreach (var item in channel.OpenAsync<T>(cancellationToken))
                {
                    await JsonSerializer.SerializeAsync(context.Response.Body, item, DefaultJsonOptions, cancellationToken);
                    await context.Response.WriteAsync("\n", cancellationToken);
                }
            }

        }

        static readonly JsonSerializerOptions DefaultJsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Strict)
        {
            WriteIndented = false,
            Converters =
            {
                new SystemTypeJsonConverter()
            }
        };

        /// <summary>
        /// Maps the Gip pipeline host to the specified prefix.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static IEndpointRouteBuilder MapPipelineHost(this IEndpointRouteBuilder builder, RoutePattern prefix)
        {
            var g = builder.MapGroup(prefix);
            g.MapGet("f/{functionId}", GetFunctionAsync);
            g.MapPost("f/{functionId}", PostFunctionAsync);
            g.MapGet("c/{channelId}", GetChannelAsync);
            return builder;
        }

        /// <summary>
        /// Maps the Gip pipeline host to the specified prefix.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static IEndpointRouteBuilder MapPipelineHost(this IEndpointRouteBuilder builder, string prefix)
        {
            return MapPipelineHost(builder, RoutePatternFactory.Parse(prefix));
        }

        /// <summary>
        /// Maps the Gip pipeline host.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IEndpointRouteBuilder MapPipelineHost(this IEndpointRouteBuilder builder)
        {
            return MapPipelineHost(builder, "");
        }

        /// <summary>
        /// Gets the schema information for a function.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="host"></param>
        /// <param name="functionId"></param>
        /// <returns></returns>
        static async Task GetFunctionAsync(HttpContext context, Guid functionId, [FromServices] IPipelineContext host)
        {
            if (host.TryGetFunction(functionId, out var func) == false)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            context.Response.StatusCode = StatusCodes.Status200OK;
        }

        /// <summary>
        /// Begins a new function call. The first JSONL line records the call result. The response ends when the function has completely ended.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="functionId"></param>
        /// <param name="host"></param>
        /// <param name="body"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        static async Task PostFunctionAsync(HttpContext context, Guid functionId, [FromBody] CallRequest body, [FromServices] IPipelineContext host, CancellationToken cancellationToken)
        {
            if (host.TryGetFunction(functionId, out var func) == false)
            {
                context.Response.StatusCode = 404;
                return;
            }

            var sources = ImmutableArray.CreateBuilder<Abstractions.SourceParameter>(func.Schema.Sources.Length);
            for (int i = 0; i < func.Schema.Sources.Length; i++)
            {
                var s = func.Schema.Sources[i];
                var b = body.Sources[i];

                // parameter specifies a remote URI
                if (b.Remote is { } uri)
                {
                    sources.Add(new RemoteSourceParameter(uri));
                }

                // parameter includes the set of values
                if (b.Static is { } signals)
                {
                    var v = ImmutableArray.CreateBuilder<object?>(b.Static.Length);
                    foreach (var signal in signals)
                        v.Add(signal.Deserialize(s.Signal));

                    sources.Add(new StaticSourceParameter(v.ToImmutable()));
                }
            }

            // initiates the call, this never exits unless cancelled
            using var call = await func.CallAsync(context.RequestServices, sources.ToImmutable(), cancellationToken);

            // collect the output parameters
            var outputs = new CallOutputParameter[call.Outputs.Length];
            for (int i = 0; i < call.Outputs.Length; i++)
                outputs[i] = new CallOutputParameter() { Uri = new Uri(new Uri(context.Request.GetEncodedUrl()), $"../c/{call.Outputs[i].Id}") };

            // set response types
            context.Response.ContentType = "application/jsonl";
            context.Response.StatusCode = 200;

            // output the first line, which is the output argument channels
            await JsonSerializer.SerializeAsync(context.Response.Body, new CallResponse() { Outputs = outputs }, DefaultJsonOptions, cancellationToken);
            await context.Response.WriteAsync("\n", cancellationToken);
            await context.Response.Body.FlushAsync(cancellationToken);

            try
            {
                // await until the task is cancelled
                var tcs = new TaskCompletionSource();
                cancellationToken.Register(tcs.SetCanceled);
                await tcs.Task;
            }
            catch (TaskCanceledException)
            {
                // ignore
            }
        }

        /// <summary>
        /// Reads a channel's signals until completion.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="channelId"></param>
        /// <param name="host"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        static async Task GetChannelAsync(HttpContext context, Guid channelId, [FromServices] IPipelineContext host, CancellationToken cancellationToken)
        {
            if (host.TryGetChannel(channelId, out var channel) == false)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            // set response types
            context.Response.ContentType = "application/jsonl";
            context.Response.StatusCode = StatusCodes.Status200OK;

            // we use a generic writer so we can invoke the typed write methods
            await ((IJsonChannelWriter)Activator.CreateInstance(typeof(JsonChannelWriter<>).MakeGenericType(channel.Schema.Signal))!).WriteChannelAsync(context, channel, cancellationToken);
        }

    }

}
