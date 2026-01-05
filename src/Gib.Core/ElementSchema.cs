using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Gip.Abstractions;

namespace Gib.Core
{

    public record class ElementSchema(ImmutableArray<ElementPropertySchema> Properties)
    {

        /// <summary>
        /// Derives a <see cref="ElementSchema"/> from a CLR type.
        /// </summary>
        /// <param name="elementType"></param>
        /// <returns></returns>
        public static ElementSchema GetElementSchema(Type elementType)
        {
            if (elementType.IsAssignableTo(typeof(IElement)) == false)
                throw new ArgumentException("Element type must implement IElement.");

            var attr = elementType.GetCustomAttribute<ElementAttribute>(false);
            if (attr is null)
                throw new ArgumentException("Element type must be decorated with ElementAttribute.");

            return new ElementSchema(GetElementPropertySchemaArray(elementType));
        }

        /// <summary>
        /// Derives a <see cref="ElementPropertySchema"/> array for the available members of the given type.
        /// </summary>
        /// <param name="elementType"></param>
        /// <returns></returns>
        static ImmutableArray<ElementPropertySchema> GetElementPropertySchemaArray(Type elementType)
        {
            var builder = ImmutableArray.CreateBuilder<ElementPropertySchema>();

            foreach (var fieldInfo in elementType.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance))
                if (TryGetElementPropertySchema(fieldInfo, out var schema))
                    builder.Insert(schema.Index, schema);

            foreach (var propertyInfo in elementType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance))
                if (TryGetElementPropertySchema(propertyInfo, out var schema))
                    builder.Insert(schema.Index, schema);

            return builder.MoveToImmutable();
        }

        /// <summary>
        /// Attempts to derive a <see cref="ElementPropertySchema"/> from the given <see cref="FieldInfo"/>.
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        static bool TryGetElementPropertySchema(FieldInfo fieldInfo, [NotNullWhen(true)] out ElementPropertySchema? schema)
        {
            return TryGetElementPropertySchema(fieldInfo, fieldInfo.FieldType, out schema);
        }

        /// <summary>
        /// Attempts to derive a <see cref="ElementPropertySchema"/> from the given <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        static bool TryGetElementPropertySchema(PropertyInfo propertyInfo, [NotNullWhen(true)] out ElementPropertySchema? schema)
        {
            return TryGetElementPropertySchema(propertyInfo, propertyInfo.PropertyType, out schema);
        }

        /// <summary>
        /// Attempts to derive a <see cref="ElementPropertySchema"/> from the given <see cref="MemberInfo"/> with the specified type.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="propertyType"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        static bool TryGetElementPropertySchema(MemberInfo memberInfo, Type propertyType, [NotNullWhen(true)] out ElementPropertySchema? schema)
        {
            schema = null;

            var name = memberInfo.Name;
            var attr = memberInfo.GetCustomAttribute<ElementPropertyAttribute>(true);
            if (attr is null)
                return false;

            schema = new ElementPropertySchema(attr.Index, propertyType);
            return true;
        }

        /// <summary>
        /// Returns a <see cref="FunctionSchema"/> that represents how this <see cref="ElementSchema"/> should be mapped to a Gip function.
        /// </summary>
        /// <returns></returns>
        public FunctionSchema ToFunctionSchema()
        {
            throw new NotImplementedException();
        }

    }

}
