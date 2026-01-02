using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Gib.Hosting.Handlers.Library
{

    /// <summary>
    /// Implements <see cref="AssemblyLoadContext"/> for a expanded library assembly.
    /// </summary>
    class LibraryLoadContext : AssemblyLoadContext
    {

        readonly AssemblyDependencyResolver _resolver;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="assemblyPath"></param>
        public LibraryLoadContext(string assemblyPath) : base($"gib::{assemblyPath}", true)
        {
            _resolver = new AssemblyDependencyResolver(assemblyPath);
            this.Unloading += LibraryLoadContext_Unloading;
        }

        private void LibraryLoadContext_Unloading(AssemblyLoadContext obj)
        {
            Console.WriteLine("unloaded");
        }

        /// <inheritdoc />
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
                return LoadFromAssemblyPath(assemblyPath);

            return null;
        }

        /// <inheritdoc />
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
                return LoadUnmanagedDllFromPath(libraryPath);

            return IntPtr.Zero;
        }

    }

}
