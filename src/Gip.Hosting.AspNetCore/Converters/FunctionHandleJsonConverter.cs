using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Gip.Abstractions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Gip.Hosting.AspNetCore.Converters
{

    public class FunctionHandleJsonConverter : JsonConverter<IFunctionHandle>
    {

        readonly HttpContext context;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public FunctionHandleJsonConverter(HttpContext context)
        {
            this.context = context;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsAssignableTo(typeof(IFunctionHandle));
        }

        public override IFunctionHandle? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IFunctionHandle value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, new Uri(new Uri(context.Request.GetEncodedUrl()), $"../f/{value.Id}"), options);
        }

    }

}
