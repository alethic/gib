using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using BernhardHaus.Collections.WeakDictionary;

using Gib.Reflection;

namespace Gib.Hosting.Handlers.Library
{

    /// <summary>
    /// Maintains a set of active library containers and routes elemenet requests to them.
    /// </summary>
    public class LibraryHost
    {

        readonly WeakDictionary<string, LibraryContainer> _loaded;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public LibraryHost()
        {
            _loaded = new WeakDictionary<string, LibraryContainer>();
        }

        /// <summary>
        /// Gets the given element type from the specified local library path. The passed Element Type URI must be a 'file' schema URI.
        /// </summary>
        /// <param name="elementTypeUri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async ValueTask<ElementTypeDescriptor?> GetElementTypeAsync(Uri elementTypeUri, CancellationToken cancellationToken)
        {
            if (elementTypeUri.Scheme != "file")
                return null;

            var s = elementTypeUri.AbsolutePath.Split('!', 2);
            if (s.Length != 2)
                throw new FormatException("Invalid element type URI: path must terminate with a exclamation point followed by the type name.");

            var assemblyPath = s[0];
            if (File.Exists(assemblyPath) == false)
                throw new InvalidOperationException("Invalid element type URI: assembly path does not exist.");

            var typeName = s[1];
            if (typeName.Length < 1)
                throw new FormatException("Invalid element type URI: type name must be at least one character.");

            return GetElementTypeAsync(assemblyPath, typeName);
        }

        /// <summary>
        /// Gets the element type descriptor from the given library path with the given type name.
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        ElementTypeDescriptor? GetElementTypeAsync(string assemblyPath, string typeName)
        {
            return GetOrCreateContainer(assemblyPath).GetElementTypes().Where(i => i.TypeName == typeName).Select(i => ToDescriptor(assemblyPath, i)).FirstOrDefault();
        }

        /// <summary>
        /// Generates a <see cref="ElementTypeDescriptor"/> for the reflected element type.
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        ElementTypeDescriptor ToDescriptor(string assemblyPath, ElementType type)
        {
            return new ElementTypeDescriptor(new Uri($"dotnet:lib:file://{assemblyPath}!{type.TypeName}"), type.Properties.Select(ToDescriptor).ToImmutableArray());
        }

        /// <summary>
        /// Generates an <see cref="ElementPropertyDescriptor"/> for the reflected element property.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        ElementPropertyDescriptor ToDescriptor(ElementProperty property)
        {
            return new ElementPropertyDescriptor(property.Name);
        }

        /// <summary>
        /// Gets or creates a reference to the container task for the given library path.
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <returns></returns>
        LibraryContainer GetOrCreateContainer(string assemblyPath)
        {
            lock (_loaded)
            {
                if (_loaded.TryGetValue(assemblyPath, out var container) == false)
                    _loaded[assemblyPath] = container = new LibraryContainer(assemblyPath);

                return container;
            }
        }

    }

}
