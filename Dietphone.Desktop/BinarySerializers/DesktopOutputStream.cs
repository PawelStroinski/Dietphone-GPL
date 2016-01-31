using System.IO;

namespace Dietphone.BinarySerializers
{
    public class DesktopOutputStream : OutputStream
    {
        public Stream Stream { get; private set; }

        public DesktopOutputStream(Stream stream)
        {
            Stream = stream;
        }

        public void Commit(long size)
        {
        }
    }
}
