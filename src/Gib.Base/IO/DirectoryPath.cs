using ProtoBuf;

namespace Gib.Base.IO
{

    [ProtoContract]
    public record struct DirectoryPath(
        [property: ProtoMember(1, Name = "absolutePath")] string AbsolutePath
    );

}
