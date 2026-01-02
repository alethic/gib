using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Gip.Abstractions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Gip.Hosting.AspNetCore.Converters
{

    public class ChannelHandleJsonConverter : JsonConverter<IChannelHandle>
    {

        readonly HttpContext context;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public ChannelHandleJsonConverter(HttpContext context)
        {
            this.context = context;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsAssignableTo(typeof(IChannelHandle));
        }

        public override IChannelHandle? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IChannelHandle value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, new Uri(new Uri(context.Request.GetEncodedUrl()), $"../c/{value.Id}"), options);
        }

    }

}
