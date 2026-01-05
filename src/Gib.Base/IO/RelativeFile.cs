using ProtoBuf;

namespace Gib.Base.IO
{

    [ProtoContract]
    public record struct RelativeFile(
        [property: ProtoMember(1)] string AbsolutePath,
        [property: ProtoMember(2)] string RelativePath,
        [property: ProtoMember(3)] FileStat Statistics
    )
    {

        public static RelativeFile FromPath(string absolutePath, string relativePath)
        {
            return new RelativeFile(absolutePath, relativePath, FileStat.Read(absolutePath));
        }

    }

}
