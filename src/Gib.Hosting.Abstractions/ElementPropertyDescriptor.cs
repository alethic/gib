using System.Text.Json.Serialization;

namespace Gib.Hosting
{

    public record class ElementPropertyDescriptor
    {

        readonly string _name;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="name"></param>
        public ElementPropertyDescriptor(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name => _name;

    }

}