using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;

using Gib.Reflection;

namespace Gib.Hosting.Handlers.Library
{

    /// <summary>
    /// Represents a reference to a library directory.
    /// </summary>
    class LibraryContainer : IDisposable
    {

        readonly string _assemblyPath;

        LibraryLoadContext? _loadContext;
        RuntimeElementReflectionContext? _reflectionContext;
        Assembly? _assembly;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="assemblyPath"></param>
        public LibraryContainer(string assemblyPath)
        {
            _assemblyPath = assemblyPath;
        }

        /// <summary>
        /// Gets the element types from this container.
        /// </summary>
        /// <returns></returns>
        public ImmutableList<ElementType> GetElementTypes()
        {
            return GetOrCreateReflectionContext().GetElementTypes(_assembly ??= SafeLoadAssembly(GetAssemblyName(_assemblyPath)) ?? throw new InvalidOperationException("Could not load assembly."));
        }

        /// <summary>
        /// Gets the assembly name of the assembly at the specifed path.
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <returns></returns>
        string GetAssemblyName(string assemblyPath)
        {
            return Path.GetFileNameWithoutExtension(assemblyPath);
        }

        /// <summary>
        /// Safely attempts to load the assembly with the given name.
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        Assembly SafeLoadAssembly(string assemblyName)
        {
            try
            {
                return GetOrCreateLoadContext().LoadFromAssemblyName(new AssemblyName(assemblyName));
            }
            catch (FileNotFoundException e)
            {
                throw new InvalidOperationException($"Could not load assembly {assemblyName}.", e);
            }
            catch (FileLoadException e)
            {
                throw new InvalidOperationException($"Could not load assembly {assemblyName}.", e);
            }
            catch (BadImageFormatException e)
            {
                throw new InvalidOperationException($"Could not load assembly {assemblyName}.", e);
            }
        }

        /// <summary>
        /// Gets or creates the library load context.
        /// </summary>
        /// <returns></returns>
        LibraryLoadContext GetOrCreateLoadContext()
        {
            if (_loadContext is null)
                lock (this)
                    _loadContext = new LibraryLoadContext(_assemblyPath);

            return _loadContext;
        }

        /// <summary>
        /// Gets or creates the library load context.
        /// </summary>
        /// <returns></returns>
        RuntimeElementReflectionContext GetOrCreateReflectionContext()
        {
            if (_reflectionContext is null)
                lock (this)
                    _reflectionContext = new RuntimeElementReflectionContext(GetOrCreateLoadContext());

            return _reflectionContext;
        }

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        public void Dispose()
        {
            lock (this)
            {
                _loadContext?.Unload();
                _loadContext = null;
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes the instance.
        /// </summary>
        ~LibraryContainer()
        {
            Dispose();
        }

    }

}
