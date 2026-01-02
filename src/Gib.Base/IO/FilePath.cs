using ProtoBuf;

namespace Gib.Base.IO
{

    [ProtoContract]
    public record struct FilePath(
        [property: ProtoMember(1, Name = "absolutePath")] string AbsolutePath
    );

}
