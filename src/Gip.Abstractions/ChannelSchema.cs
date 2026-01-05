using System;

using ProtoBuf.Meta;

namespace Gip.Abstractions
{

    /// <summary>
    /// Describes the schema of a channel.
    /// </summary>
    /// <param name="Signal"></param>
    public record class ChannelSchema(MetaType Signal)
    {

        /// <summary>
        /// Returns a <see cref="ChannelSchema"/> suitable for representing messages of the given CLR type.
        /// </summary>
        /// <param name="clrType"></param>
        /// <returns></returns>
        public static ChannelSchema FromClrType(Type clrType)
        {
            return new ChannelSchema(RuntimeTypeModel.Default.Add(clrType, true));
        }

        /// <summary>
        /// Returns a <see cref="ChannelSchema"/> suitable for representing messages of the given CLR type.
        /// </summary>
        /// <returns></returns>
        public static ChannelSchema FromClrType<T>()
        {
            return new ChannelSchema(RuntimeTypeModel.Default.Add(typeof(T), true));
        }

    }

}
