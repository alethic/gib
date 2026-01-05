using System.Collections.Immutable;

using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class SetRemoveManySignal<T> : SetSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="items"></param>
        public SetRemoveManySignal(ImmutableArray<T> items)
        {
            Items = items;
        }

        /// <summary>
        /// Gets the items that were removed.
        /// </summary>
        [ProtoMember(1)]
        public ImmutableArray<T> Items { get; }

    }

}
