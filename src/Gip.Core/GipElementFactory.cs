using System.Collections.Generic;
using System.Linq;

namespace Gip.Core
{

    public abstract class GipElementFactory
    {

        readonly List<GipPadTemplate> padTemplates = new();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected GipElementFactory()
        {

        }

        /// <summary>
        /// Gets the set of pads that would be available on the element.
        /// </summary>
        public IReadOnlyList<GipPadTemplate> PadTemplates => padTemplates;

        /// <summary>
        /// Adds a new pad template.
        /// </summary>
        /// <param name="template"></param>
        public void AddPadTemplate(GipPadTemplate template)
        {
            padTemplates.Add(template);
        }

        /// <summary>
        /// Removes an existing pad template.
        /// </summary>
        /// <param name="template"></param>
        public void RemovePadTemplate(GipPadTemplate template)
        {
            padTemplates.Remove(template);
        }

        /// <summary>
        /// Retrieves a <see cref="GipPadTemplate"/> with the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GipPadTemplate? GetPadTemplate(string name)
        {
            return padTemplates.FirstOrDefault(i => i.Name == name);
        }

        /// <summary>
        /// Creates a new instance of the element.
        /// </summary>
        /// <returns></returns>
        public abstract GipElement Create();

    }

}
