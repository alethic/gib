using System;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Gib.Hosting
{

    public record class ElementTypeDescriptor
    {

        readonly Uri _uri;
        readonly ImmutableArray<ElementPropertyDescriptor> _properties;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="properties"></param>
        public ElementTypeDescriptor(Uri uri, ImmutableArray<ElementPropertyDescriptor> properties)
        {
            _uri = uri;
            _properties = properties;
        }

        /// <summary>
        /// Gets the canonical element type URI.
        /// </summary>
        [JsonPropertyName("uri")]
        public Uri Uri => _uri;

        /// <summary>
        /// Gets the available element properties of the element type.
        /// </summary>
        [JsonPropertyName("properties")]
        public ImmutableArray<ElementPropertyDescriptor> Properties => _properties;

    }

}
