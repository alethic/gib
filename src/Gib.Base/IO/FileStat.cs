using System;
using System.IO;

using ProtoBuf;

namespace Gib.Base.IO
{

    [ProtoContract]
    public record struct FileStat(
        [property: ProtoMember(1, Name = "ctime")] DateTime CreatedTime,
        [property: ProtoMember(2, Name = "mtime")] DateTime ModifiedTime,
        [property: ProtoMember(3, Name = "atime")] DateTime AccessedTime)
    {

        /// <summary>
        /// Reads the file stats of the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FileStat Read(string path)
        {
            return new FileStat(File.GetCreationTimeUtc(path), File.GetLastWriteTimeUtc(path), File.GetLastAccessTimeUtc(path));
        }

    }

}
