using System;

namespace Gip.Abstractions
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public class GenericProtoIncludeAttribute : Attribute
    {

        /// <summary>
        /// Initialites a new instance.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="knownGenericType"></param>
        /// <param name="typeParameterMap"></param>
        public GenericProtoIncludeAttribute(int tag, Type knownGenericType, params int[] typeParameterMap)
        {
            Tag = tag;
            KnownGenericType = knownGenericType;
            TypeParamterMap = typeParameterMap;
        }

        /// <summary>
        /// Gets the unique index (within the type) that will identify this data.
        /// </summary>
        public int Tag { get; set; }

        /// <summary>
        /// Gets the additional type to serialize/deserialize.
        /// </summary>
        public Type KnownGenericType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int[] TypeParamterMap { get; set; }

    }

}