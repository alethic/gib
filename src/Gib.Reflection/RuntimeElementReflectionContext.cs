using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

using Gib.Core.Elements;

namespace Gib.Reflection
{

    /// <summary>
    /// Provides methods to resolve and cache element type information from .NET assemblies.
    /// </summary>
    public class RuntimeElementReflectionContext
    {

        readonly AssemblyLoadContext _context;
        readonly Assembly _core;
        readonly Type _elementAttrType;
        readonly Type _propertyAttrType;
        readonly PropertyInfo _propertyAttrNameProp;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public RuntimeElementReflectionContext(AssemblyLoadContext context)
        {
            _context = context;
            _core = _context.LoadFromAssemblyName(new AssemblyName("Gib.Core"));
            _elementAttrType = _core.GetType(typeof(ElementAttribute).FullName!) ?? throw new InvalidOperationException($"Could not load {typeof(ElementAttribute).FullName!}.");
            _propertyAttrType = _core.GetType(typeof(PropertyAttribute).FullName!) ?? throw new InvalidOperationException($"Could not load {typeof(PropertyAttribute).FullName!}.");
            _propertyAttrNameProp = _propertyAttrType.GetProperty(nameof(PropertyAttribute.PropertyName)) ?? throw new InvalidOperationException($"Could not load {typeof(PropertyAttribute).FullName!}.");
        }

        /// <summary>
        /// Resolves element types from the specified entry assembly.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public ImmutableList<ElementType> GetElementTypes(Assembly assembly)
        {
            return assembly.GetTypes().Select(ReflectType).Where(i => i != null).Select(i => i!).ToImmutableList();
        }

        /// <summary>
        /// Reflects the given <see cref="Type"/> into a <see cref="ElementType"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        ElementType? ReflectType(Type type)
        {
            if (type.IsPublic == false)
                return null;

            var elementAttr = type.GetCustomAttribute(_elementAttrType, false);
            if (elementAttr is null)
                return null;

            var properties = ImmutableArray.CreateBuilder<ElementProperty>();
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
                if (ReflectProperty(property) is ElementProperty p)
                    properties.Add(p);

            return new ElementType(type.Assembly.GetName().Name!, type.FullName!, properties.ToImmutable());
        }

        /// <summary>
        /// Reflects the given <see cref="PropertyInfo"/> into a <see cref="ElementProperty"/>.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        ElementProperty? ReflectProperty(PropertyInfo property)
        {
            if (property.GetGetMethod() is null)
                return null;

            var propertyAttr = property.GetCustomAttribute(_propertyAttrType, true);
            if (propertyAttr is null)
                return null;

            if (_propertyAttrNameProp.GetValue(propertyAttr) is not string name)
                name = property.Name;

            return new ElementProperty(name);
        }

    }

}
