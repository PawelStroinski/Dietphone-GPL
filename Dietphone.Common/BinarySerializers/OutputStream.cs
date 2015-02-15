using System.IO;

namespace Dietphone.BinarySerializers
{
    public interface OutputStream
    {
        Stream Stream { get; }
        void Commit(long size);
    }
}
