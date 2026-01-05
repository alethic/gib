using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using ProtoBuf.Meta;

namespace Gip.Abstractions
{

    public class FunctionSchemaBuilder
    {

        readonly RuntimeTypeModel _model;
        readonly ImmutableArray<ChannelSchema>.Builder _sources = ImmutableArray.CreateBuilder<ChannelSchema>();
        readonly ImmutableArray<ChannelSchema>.Builder _outputs = ImmutableArray.CreateBuilder<ChannelSchema>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="model"></param>
        public FunctionSchemaBuilder(RuntimeTypeModel model)
        {
            _model = model;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="model"></param>
        public FunctionSchemaBuilder() : this(RuntimeTypeModel.Create())
        {

        }

        /// <summary>
        /// Adds the given type to the model.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        MetaType AddToModel(Type type)
        {
            // we add the hierarchy
            AddToModel(type, []);

            // call add a second time to retrieve
            return _model.Add(type, true);
        }

        /// <summary>
        /// Adds the given type to the model.
        /// </summary>
        /// <param name="type"></param>
        void AddToModel(Type type, HashSet<Type> hs)
        {
            // we do not need to deal with this
            if (type.FullName == "System.Object")
                return;

            // disallow cycles
            if (hs.Add(type) == false)
                return;

            // recurse through base types
            if (type.BaseType is Type baseType)
                AddToModel(baseType, hs);

            var metaType = _model.Add(type, true);

            if (type.IsConstructedGenericType)
            {
                foreach (var attr in type.GetCustomAttributes<GenericProtoIncludeAttribute>(true))
                {
                    var genericType = attr.KnownGenericType;
                    var sourceTypes = type.GetGenericArguments();
                    var outputTypes = new Type[attr.TypeParamterMap.Length];
                    for (int i = 0; i < attr.TypeParamterMap.Length; i++)
                        outputTypes[i] = sourceTypes[attr.TypeParamterMap[i]];

                    // we attempt to add the subtype, ignoring recursion
                    var subType = attr.KnownGenericType.MakeGenericType(outputTypes);
                    AddToModel(subType, hs);
                    var subMetaType = _model.Add(subType, true);

                    // check if we already have that specific subtype registered
                    if (metaType.GetSubtypes().Any(i => i.DerivedType == subMetaType) == false)
                        metaType.AddSubType(attr.Tag, subType);
                }
            }
        }

        /// <summary>
        /// Adds a new source channel to the schema.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public FunctionSchemaBuilder Source(Type type)
        {
            _sources.Add(new ChannelSchema(AddToModel(type)));
            return this;
        }

        /// <summary>
        /// Adds a new source channel to the schema.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public FunctionSchemaBuilder Source<T>()
        {
            return Source(typeof(T));
        }

        /// <summary>
        /// Adds a new source channel to the schema.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public FunctionSchemaBuilder Output(Type type)
        {
            _outputs.Add(new ChannelSchema(AddToModel(type)));
            return this;
        }

        /// <summary>
        /// Adds a new output channel to the schema.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public FunctionSchemaBuilder Output<T>()
        {
            return Output(typeof(T));
        }

        /// <summary>
        /// Builds a new <see cref="FunctionSchema"/>.
        /// </summary>
        /// <returns></returns>
        public FunctionSchema Build()
        {
            return new FunctionSchema(_model, _sources.ToImmutable(), _outputs.ToImmutable());
        }

    }

}