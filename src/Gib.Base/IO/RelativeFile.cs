using ProtoBuf;

namespace Gib.Base.IO
{

    [ProtoContract]
    public record struct RelativeFile(
        [property: ProtoMember(1, Name = "absolutePath")] string AbsolutePath,
        [property: ProtoMember(2, Name = "relativePath")] string RelativePath
    );

}
