using Gib.Base.IO;

using ProtoBuf;

namespace Gib.Base.IO
{

    [ProtoContract]
    public record struct AbsoluteFile(
        [property: ProtoMember(1)] string AbsolutePath,
        [property: ProtoMember(2)] FileStat Statistics
    )
    {

        public static AbsoluteFile FromPath(string absolutePath)
        {
            return new AbsoluteFile(absolutePath, FileStat.Read(absolutePath));
        }

    }

}
