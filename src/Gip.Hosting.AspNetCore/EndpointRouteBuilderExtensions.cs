using System;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Core.Clients.Http.Json;
using Gip.Hosting.AspNetCore.Converters;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Gip.Hosting.AspNetCore
{

    /// <summary>
    /// Provides extensions for mapping the <see cref="AspNetCorePipeline"/> to endpoints.
    /// </summary>
    public static class EndpointRouteBuilderExtensions
    {

        /// <summary>
        /// Non-generic interface for JSON channel serializers.
        /// </summary>
        interface IJsonChannelSerializer
        {

            ValueTask SerializeAsync(HttpContext context, IReadableChannelHandle channel, CancellationToken cancellationToken);

        }

        /// <summary>
        /// Generic version of writer. Allows typed invocation of reader.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class JsonChannelSerializer<T> : IJsonChannelSerializer
        {

            public async ValueTask SerializeAsync(HttpContext context, IReadableChannelHandle channel, CancellationToken cancellationToken)
            {
                await foreach (var item in channel.OpenRead<T>(cancellationToken))
                {
                    await JsonSerializer.SerializeAsync(context.Response.Body, item, DefaultJsonOptions, cancellationToken);
                    await context.Response.WriteAsync("\n", cancellationToken);
                }
            }

        }

        /// <summary>
        /// Non-generic interface for JSON channel deserializers.
        /// </summary>
        interface IJsonChannelDeserializer
        {

            void Deserialize(JsonNode[] nodes, IWritableChannelHandle channel, CancellationToken cancellationToken);

        }

        /// <summary>
        /// Generic version of deserializer. Allows typed invocation of writer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class JsonChannelDeserializer<T> : IJsonChannelDeserializer
        {

            public void Deserialize(JsonNode[] nodes, IWritableChannelHandle channel, CancellationToken cancellationToken)
            {
                using var writer = channel.OpenWrite<T>();
                foreach (var node in nodes)
                    writer.Write(JsonSerializer.Deserialize<T>(node, DefaultJsonOptions) ?? throw new InvalidOperationException());
            }

        }

        /// <summary>
        /// Default JSON options for serialization of our responses.
        /// </summary>
        static readonly JsonSerializerOptions DefaultJsonOptions = new(JsonSerializerDefaults.Strict)
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
        /// <param name="pipeline"></param>
        /// <param name="body"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        static async Task PostFunctionAsync(HttpContext context, Guid functionId, [FromBody] CallRequest body, [FromServices] IPipelineContext pipeline, CancellationToken cancellationToken)
        {
            if (pipeline.TryGetFunction(functionId, out var func) == false)
            {
                context.Response.StatusCode = 404;
                return;
            }

            var sources = ImmutableArray.CreateBuilder<IReadableChannelHandle?>(func.Schema.Sources.Length);
            for (int i = 0; i < func.Schema.Sources.Length; i++)
            {
                var s = func.Schema.Sources[i];
                var b = body.Sources[i];

                // parameter specifies a remote URI
                if (b.Remote is { } uri)
                {
                    throw new NotImplementedException();
                }

                // parameter includes the set of values
                if (b.Static is { } signals)
                {
                    var channel = pipeline.CreateChannel(s);
                    ((IJsonChannelDeserializer)Activator.CreateInstance(typeof(JsonChannelDeserializer<>).MakeGenericType(s.Signal))!).Deserialize(signals, channel, cancellationToken);
                    sources.Add(channel);
                }
            }

            var outputs = ImmutableArray.CreateBuilder<IWritableChannelHandle?>(func.Schema.Outputs.Length);
            for (int i = 0; i < func.Schema.Outputs.Length; i++)
                sources.Add(null);

            // initiates the call
            await using var call = await func.CallAsync(sources.MoveToImmutable(), outputs.MoveToImmutable(), cancellationToken);

            // collect the output parameters
            var outputJson = new CallOutputParameter[call.Outputs.Length];
            for (int i = 0; i < call.Outputs.Length; i++)
                outputJson[i] = new CallOutputParameter() { Uri = pipeline.GetChannelUri(call.Outputs[i]) };

            // set response types
            context.Response.ContentType = "application/jsonl";
            context.Response.StatusCode = 200;

            // output the first line, which is the output argument channels
            await JsonSerializer.SerializeAsync(context.Response.Body, new CallResponse() { Outputs = outputJson }, DefaultJsonOptions, cancellationToken);
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
            await ((IJsonChannelSerializer)Activator.CreateInstance(typeof(JsonChannelSerializer<>).MakeGenericType(channel.Schema.Signal))!).SerializeAsync(context, channel, cancellationToken);
        }

    }

}
